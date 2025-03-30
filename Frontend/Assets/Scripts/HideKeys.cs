using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;

public class ClickSelect : MonoBehaviour{
    [SerializeField] private GameObject UserKeyboard; // Reference to the XRKeyboard
    [SerializeField] private GameObject PassKeyboard;
    public void ShowKeysUser(){
        // This method will be called when the object is clicked
        // It will hide the XRKeyboard
        if (UserKeyboard != null)
        {
            UserKeyboard.SetActive(true); // Hide the keyboard
        }
        else
        {
            Debug.LogError("XRKeyboard reference not set.");
        }
    }
    public void HideKeysUser(){
        // This method will be called when the object is clicked
        // It will hide the XRKeyboard
        if (UserKeyboard != null)
        {
            UserKeyboard.SetActive(false); // Hide the keyboard
        }
        else
        {
            Debug.LogError("XRKeyboard reference not set.");
        }
    }
    public void ShowKeysPass(){
        // This method will be called when the object is clicked
        // It will hide the XRKeyboard
        if (PassKeyboard != null)
        {
            PassKeyboard.SetActive(true); // Hide the keyboard
        }
        else
        {
            Debug.LogError("XRKeyboard reference not set.");
        }
    }
    public void HideKeysPass(){
        // This method will be called when the object is clicked
        // It will hide the XRKeyboard
        if (PassKeyboard != null)
        {
            PassKeyboard.SetActive(false); // Hide the keyboard
        }
        else
        {
            Debug.LogError("XRKeyboard reference not set.");
        }
    }
}