using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.CognitiveServices.Speech;
using TMPro;

public class SttManager : MonoBehaviour
{
    [SerializeField] private string azureSubscriptionKey; // Azure Key 입력
    [SerializeField] private string azureServiceRegion; // Azure Region 입력
    [SerializeField] private TextMeshProUGUI sttTextObj;
    private SpeechRecognizer _recognizer;
    private object _threadLocker = new object();
    private string _sttText;

    void Start()
    {
        // Azure STT 환경 설정 생성
        var config = SpeechConfig.FromSubscription(this.azureSubscriptionKey, this.azureServiceRegion);
        config.SpeechRecognitionLanguage = "ko-KR";

        // Azure STT 음성 인식 인스턴스 생성
        this._recognizer = new SpeechRecognizer(config);
        this._recognizer.Recognizing += RecognizingHandler;
        
        // Azure STT 음성 인식 시작
        this._recognizer.StartContinuousRecognitionAsync();
    }

    void Update()
    {
        // 화면에 텍스트 표시
        this.sttTextObj.text = this._sttText;
    }

    void OnDestroy()
    {
        // Azure STT 음성 인식 종료
        this._recognizer.StopContinuousRecognitionAsync();
    }

    private void RecognizingHandler(object sender, SpeechRecognitionEventArgs e)
    {
        lock (_threadLocker) // 비동기 실행되기 때문에 쓰레드 Lock한 후 로직 처리
        {
            string msg = e.Result.Text;
            Debug.Log($"recognizing : {msg}");
            this._sttText = msg;
        }
    }
}
