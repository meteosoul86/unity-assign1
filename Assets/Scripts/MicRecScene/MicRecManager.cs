using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicRecManager : MonoBehaviour
{
    // 입력
    private string _deviceName; // 마이크 디바이스 이름
    private AudioClip _mic; // 마이크 AudioClip
    private int _micChannelCount; // 마이크 채널 수
    private readonly int _sampleRate = 44100; // 샘플링 주기 - 1초에 SampleRate만큼 
    
    // 읽기
    private int _readingEndSamplePos = 0; // 읽기 완료된 샘플링 위치
    private float[] _sampleDatas = null; // 샘플 데이터를 담을 배열
    
    // 재생
    [SerializeField] private AudioSource audioSource; // 재생에 사용할 Audio Source
    
    void Start()
    {
        this._deviceName = Microphone.devices[0]; // 첫 번째 마이크 사용
        // Debug.Log("deviceName : " + this._deviceName);
        this._mic = Microphone.Start(this._deviceName, true, 1, this._sampleRate);
        this._micChannelCount = this._mic.channels;
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
        }
        else // 현재 샘플링 위치가 읽기 완료된 샘플링 위치 이후가 아닌 경우
        {
            // 샘플 데이터 배열 비우기
            this._sampleDatas = null;
        }

        // 현재 샘플링 위치를 읽기 완료된 샘플링 위치로 저장
        this._readingEndSamplePos = currentSamplingPos;
    }

    private void PlayMic()
    {
        if (this._sampleDatas != null) // 샘플 데이터 배열에 값들이 있는 경우
        {
            // Audio Source에 AudioClip 동적 생성
            this.audioSource.clip = AudioClip.Create("Mic_Recording", this._sampleDatas.Length,
                this._micChannelCount, this._sampleRate, false);
            // 스테레오 출력으로 믹싱 설정
            this.audioSource.spatialBlend = 0; 
            
            // 샘플 데이터를 Audio Source AudioClip에 데이터로 설정
            this.audioSource.clip.SetData(this._sampleDatas, 0);
            
            if (!this.audioSource.isPlaying) // 현재 재생중이 아닌 경우
            {
                // Audio Source 재생 실행
                this.audioSource.Play();
            }
        }
    }
}
