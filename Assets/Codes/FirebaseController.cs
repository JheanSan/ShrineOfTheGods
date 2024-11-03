using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FirebaseController : MonoBehaviour
{
   public GameObject loginPanel, signupPanel;

   public InputField loginUsername, loginPassword, signupEmail, signupPassword, signupCPassword, signupUsername;

   public void OpenLoginPanel(){
       loginPanel.SetActive(true);
       signupPanel.SetActive(false);
   }

   public void OpenSignUpPanel(){
       loginPanel.SetActive(false);
       signupPanel.SetActive(true);
   }

   public void LoginUser(){
       if(string.IsNullOrEmpty(loginUsername.text) || string.IsNullOrEmpty(loginPassword.text)){
           return;
       }
   }

   public void SignUpUser(){
       if(string.IsNullOrEmpty(signupUsername.text) || string.IsNullOrEmpty(signupEmail.text) ||
          string.IsNullOrEmpty(signupPassword.text) || string.IsNullOrEmpty(signupCPassword.text)){
           return;
       }

       if(signupPassword.text != signupCPassword.text){
           return;
       }
   }
}
