using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;

public class ClickSelect : MonoBehaviour{
    [SerializeField] private GameObject UserKeyboard; // Reference to the XRKeyboard
    [SerializeField] private GameObject PassKeyboard;
    [SerializeField] private GameObject SecurityQKeyboard; // Reference to the XRKeyboard for security questions
    [SerializeField] private GameObject SecurityAKeyboard; // Reference to the XRKeyboard for security answers
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
    public void ShowKeysSecurityQ(){
        // This method will be called when the object is clicked
        // It will hide the XRKeyboard
        if (SecurityQKeyboard != null)
        {
            SecurityQKeyboard.SetActive(true); // Show the keyboard
        }
        else
        {
            Debug.LogError("XRKeyboard reference not set.");
        }
    }
    public void HideKeysSecurityQ(){
        // This method will be called when the object is clicked
        // It will hide the XRKeyboard
        if (SecurityQKeyboard != null)
        {
            SecurityQKeyboard.SetActive(false); // Hide the keyboard
        }
        else
        {
            Debug.LogError("XRKeyboard reference not set.");
        }
    }
    public void ShowKeysSecurityA(){
        // This method will be called when the object is clicked
        // It will hide the XRKeyboard
        if (SecurityAKeyboard != null)
        {
            SecurityAKeyboard.SetActive(true); // Show the keyboard
        }
        else
        {
            Debug.LogError("XRKeyboard reference not set.");
        }
    }
    public void HideKeysSecurityA(){
        // This method will be called when the object is clicked
        // It will hide the XRKeyboard
        if (SecurityAKeyboard != null)
        {
            SecurityAKeyboard.SetActive(false); // Hide the keyboard
        }
        else
        {
            Debug.LogError("XRKeyboard reference not set.");
        }
    }
}