using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ParentalGateButtonLock : MonoBehaviour
{
    [Header("Event is invoked when user answers correct")]
    public UnityEvent onCorrectAnswer;
    [Header("Event is invoked when user answers wrong")]
    public UnityEvent onWrongAnswer;
    [Header("Event is invoked when user press close button")]
    public UnityEvent onClose;
   
}
