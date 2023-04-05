using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectedMicRecManager : MonoBehaviour
{
    // 입력
    private string _deviceName; // 마이크 디바이스 이름
    private AudioClip _mic; // 마이크 AudioClip
    private int _micChannelCount; // 마이크 채널 수
    private readonly int _sampleRate = 44100; // 샘플링 주기 - 1초에 SampleRate만큼 
    
    // 읽기
    private int _readingEndSamplePos = 0; // 읽기 완료된 샘플링 위치
    private float[] _sampleDatas = null; // 샘플 데이터를 담을 배열
    private float[] _collectedSampleDatas; // 샘플 데이터를 누적해 담을 배열 
    
    // 재생
    [SerializeField] private AudioSource audioSource; // 재생에 사용할 Audio Source
    [SerializeField] private float timeLimit = 0.3f; // 누적 기준 시간
    private float _collectedTime = 0.0f; // 누적된 시간
    
    void Start()
    {
        this._deviceName = Microphone.devices[0]; // 첫 번째 마이크 사용
        // Debug.Log("deviceName : " + this._deviceName);
        this._mic = Microphone.Start(this._deviceName, true, 1, this._sampleRate);
        this._micChannelCount = this._mic.channels;
        this._collectedSampleDatas = new float[0];
    }

    void Update()
    {
        // 매 프레임마다 마이크 입력 읽기와 재생 실행
        ReadMic();
        PlayMic();
    }

    private void ReadMic()
    {
        // 현재 샘플링 위치
        int currentSamplingPos = Microphone.GetPosition(this._deviceName);
        // Debug.Log("currentSamplingPos : " + currentSamplingPos);
        // Debug.Log("currentSamplingPos : " + Math.Round(currentSamplingPos / (double) SampleRate, 1) + "s");
        
        int diff = currentSamplingPos - this._readingEndSamplePos;
        if (diff > 0) // 현재 샘플링 위치가 읽기 완료된 샘플링 위치 이후인 경우
        {
            // 샘플 데이터를 담을 배열 크기 설정
            this._sampleDatas = new float[diff * this._micChannelCount];
            // 샘플 데이터 배열에 읽기 완료된 샘플링 위치 이후의 데이터 전부 삽입
            this._mic.GetData(this._sampleDatas, this._readingEndSamplePos);
            
            // 샘플 데이터 누적 시작
            int oldLeng = this._collectedSampleDatas.Length;
            int addLeng = this._sampleDatas.Length;
            int newLeng = oldLeng + addLeng;
            
            if (oldLeng > 0) // 기존에 누적된 샘플 데이터가 있는 경우
            {
                // 기존에 누적된 샘플 데이터를 oldSampleDatas에 임시로 복사
                float[] oldSampleDatas = new float[oldLeng];
                this._collectedSampleDatas.CopyTo(oldSampleDatas, 0);
            
                // _collectedSampleDatas를 새로 선언 + oldSampleDatas의 샘플 데이터 삽입
                this._collectedSampleDatas = new float[newLeng];
                for (int i = 0; i < oldLeng; i++)
                {
                    this._collectedSampleDatas[i] = oldSampleDatas[i];
                }
            }
            else // 기존에 누적된 샘플 데이터가 없는 경우
            {
                // _collectedSampleDatas를 새로 선언
                this._collectedSampleDatas = new float[newLeng];
            }

            // GetData로 얻은 샘플 데이터 삽입
            for (int j = oldLeng; j < newLeng; j++)
            {
                this._collectedSampleDatas[j] = this._sampleDatas[j - oldLeng];
            }
            // 샘플 데이터 누적 끝
        }

        // 현재 샘플링 위치를 읽기 완료된 샘플링 위치로 저장
        this._readingEndSamplePos = currentSamplingPos;
    }

    private void PlayMic()
    {
        // 시간 누적
        this._collectedTime += Time.deltaTime;

        if (this._collectedTime >= this.timeLimit) // 누적된 시간이 누적 기준 시간을 넘는 경우
        {
            if (this._collectedSampleDatas.Length > 0) // 샘플 데이터 배열에 값들이 있는 경우
            {
                // Audio Source에 AudioClip 동적 생성
                this.audioSource.clip = AudioClip.Create("Mic_Recording", this._collectedSampleDatas.Length,
                    this._micChannelCount, this._sampleRate, false);
                // 스테레오 출력으로 믹싱 설정
                this.audioSource.spatialBlend = 0; 
            
                // 샘플 데이터를 Audio Source AudioClip에 데이터로 설정
                this.audioSource.clip.SetData(this._collectedSampleDatas, 0);
            
                if (!this.audioSource.isPlaying) // 현재 재생중이 아닌 경우
                {
                    // Audio Source 재생 실행
                    this.audioSource.Play();
                    
                    // 재생한 누적 샘플 데이터 초기화
                    this._collectedSampleDatas = new float[0];
                }
            }

            this._collectedTime = 0.0f; // 누적 시간 초기화
        }
    }
}
