using UnityEngine;
using System.Text;
using System;
using System.Collections.Generic;
using UnityEngine.Windows.Speech;

public class Recognition : MonoBehaviour
{
    
    private KeywordRecognizer KwR;
    [Header("Voice Recognized Words")]
    public List<string> possibleVoiceInputs = new List<string>();
    public _words _playerInput;
    
    
    // the words the player says
    public enum _words
    {
        PlaceHolder,
        Hello,
        Help,
        Yes,
        No
    }
    void Start()
    {
        for (int i = 0; i < _words.GetNames(typeof(_words)).Length; i++)
        {
            _words word = (_words) Enum.ToObject(typeof(_words), i);
            possibleVoiceInputs.Add(word.ToString());
        }


            KwR = new KeywordRecognizer(possibleVoiceInputs.ToArray());
            KwR.OnPhraseRecognized += OnPhraseRecognized;
            KwR.Start();        
        
    }
    
    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0} ({1}){2}", args.text, args.confidence, Environment.NewLine);
        builder.AppendFormat("\tTimestamp: {0}{1}", args.phraseStartTime, Environment.NewLine);
        builder.AppendFormat("\tDuration: {0} seconds{1}", args.phraseDuration.TotalSeconds, Environment.NewLine);
        Debug.Log(builder.ToString());

        _playerInput = (_words) Enum.Parse(typeof(_words), args.text);
        
        Debug.Log(_playerInput + " - " + (int)_playerInput); 
    }
}