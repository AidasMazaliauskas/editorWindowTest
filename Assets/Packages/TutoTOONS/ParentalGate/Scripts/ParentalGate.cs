using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ParentalGate : MonoBehaviour
{
    public bool parentalGateOn = true;
    public string questionText = "What is";
    public Text questionObj;
    public GameObject childrenObj;
    public List<UnityEngine.UI.Button> answersObj;
    [HideInInspector]
    public ParentalGate instance;
    ParentalGateButtonLock lockObj;
    List<int> answers;
    int min = 1;
    int max = 9;
    int val1, val2;
    int result;

    private float scale;
    private float velocity;
    private float smoothTime = 0.1f;

    // Start is called before the first frame update

    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        Hide();
    }

    public void Show(ParentalGateButtonLock _lockObj)
    {
        lockObj = _lockObj;
        if (!parentalGateOn)
        {
            CorrectAnswer();
            return;
        }
        ResetScale();
        childrenObj.SetActive(true);
        GenerateQuestion();
    }

    public void Hide()
    {
        childrenObj.SetActive(false);
    }
    public void Close()
    {
        if(lockObj.onClose != null)
        {
            lockObj.onClose.Invoke();
        }
        Hide();
    }
    void GenerateQuestion()
    {
        val1 = Random.Range(min, max);
        val2 = Random.Range(min, max);
        result = val1 + val2;
        questionObj.text = questionText + " " + val1 + " + " + val2 + " = ?";
        AssignAnswerValues();
    }

    int GetWrongInt()
    {
        int _wrong;
        int _val1 = Random.Range(min, max);
        int _val2 = Random.Range(min, max);
        _wrong = _val1 + _val2;
        if (_wrong == result)
        {
            return GetWrongInt();
        }
        foreach (int _val in answers)
        {
            if(_val == _wrong)
            {
                return GetWrongInt();
            }
        }
        return _wrong;
    }

    void GenerateAnswerValues()
    {
        answers = new List<int>();
        answers.Add(result);

        for (int i = 0; i < answersObj.Count-1; i++)
        {
            answers.Add(GetWrongInt());
        }
        answers.Sort();
    }

    void AssignAnswerValues()
    {
        GenerateAnswerValues();
        int i = 0;
        foreach (UnityEngine.UI.Button _answer in answersObj)
        {
            _answer.GetComponentInChildren<Text>().text = answers[i].ToString();
            i++;
        }
    }

    void CorrectAnswer()
    {
        if(lockObj.onCorrectAnswer != null)
        {
            lockObj.onCorrectAnswer.Invoke();
        }
        Hide();
    }

    void WrongAnswer()
    {
        if (lockObj.onWrongAnswer != null)
        {
            lockObj.onWrongAnswer.Invoke();
        }
        Hide();
    }

    public void SubmitAnswer(UnityEngine.UI.Button _btn)
    {
        if(_btn.GetComponentInChildren<Text>().text == result.ToString())
        {
            CorrectAnswer();
        }
        else
        {
            WrongAnswer();
        }
    }
    void ResetScale()
    {
        scale = 0;
        transform.localScale = Vector3.one * scale;
    }
    void Update()
    {
        scale = Mathf.SmoothDamp(scale, 1f, ref velocity, smoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
        transform.localScale = Vector3.one * scale;
    }
}
