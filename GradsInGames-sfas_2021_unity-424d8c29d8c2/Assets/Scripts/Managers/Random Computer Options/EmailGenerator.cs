﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct EventActivationInformation
{
    public EventActivationInformation(int Action, int NameAndPc)
    {
        ActionIndex = Action;
        NameAndPcIndex = NameAndPc;
    }
    public int ActionIndex;
    public int NameAndPcIndex;
};

[System.Serializable]
public struct Email
{
    public string NameOfEmail;
    [TextArea]
    public string OutputOfEmail;
};

[System.Serializable]
public struct EmailTemplate
{
    public string To;
    public string From;
    public string Subject;
    public string Content;
    public string Attachments;
};

public class EmailGenerator : MonoBehaviour
{
    [Header("Ref To other Generators")]
    public NameGenerator NameGen;

    [Header("Settings")]
    public int EmailsPerComputer;
    private int AmountOfComputers;

    [Header("Recorded User Activations")]
    public EventActivationInformation[,] RecorededActions;

    [Header("Developer Filler Emails")]
    public Email[] FillerEmails;
    public List<int> ActiveFillerEmails;

    [Header("Generated Emails Based Of User Pc Input")]
    public int asdasd;
    public EmailTemplate[,] GeneratedEmailTemplates;
    [Space(10)]
    public Email[,] GeneratedEmails;
    public List<int> ActiveGeneratedEmails;

    public void CustomStart(int a_amountOfComputers)
    {
        AmountOfComputers = a_amountOfComputers;

        //- Initalise List To Store Information -//
        RecorededActions = new EventActivationInformation[AmountOfComputers,2];
    }

    [ContextMenu("GenerateEmails")]
    public void GenerateEmails()
    {
        GeneratedEmails = new Email[AmountOfComputers, 2];

        for (int i = 0; i < AmountOfComputers; i++)
        {
            //- loop through possible events -//
            for (int j = 0; j < 2; j++)
            {
                if (RecorededActions[i, j].ActionIndex == -1 && RecorededActions[i, j].NameAndPcIndex == -1)
                {
                    //- Set Random Filler Email -//
                    GeneratedEmails[i, j] = FillerEmails[GetRandomFillerEmail()];
                }
                else
                {
                    //- Create Temp Email Storage -//
                    Email tempEmail = new Email();

                    //- Select Randomly what email template to use for this action -//
                    int EmailVersionIndex = Random.Range(0, 3);

                    //- Get Email Name and Output -//
                    tempEmail = EmailTemplateToEmail(GeneratedEmailTemplates[RecorededActions[i, j].ActionIndex, EmailVersionIndex], i);

                    //- Save Email To Array For Later Use -//
                    GeneratedEmails[i, j] = tempEmail;
                }
            }
        }

        //- Allows next call to reslect same emails -//
        ClearRandomSelectionMemory();
    }
    
    public string GetEmailInfo(int TitleOrBody, int PcIndex, int EmailIndex)
    {
        if (TitleOrBody == 0)
            return GeneratedEmails[PcIndex, EmailIndex].NameOfEmail;
        else
            return GeneratedEmails[PcIndex, EmailIndex].OutputOfEmail;
    }

    public Email EmailTemplateToEmail(EmailTemplate template, int PcIndex)
    {
        Email Temp = new Email();
        //- Generate Email Name -//
        Temp.NameOfEmail = template.Subject + " From: " + NameGen.GetName(PcIndex);

        //- Generate Email Output-//
        Temp.OutputOfEmail = "> " + NameGen.GetNameExcluding(PcIndex) + "\n" +
                            "> From: " + NameGen.GetName(PcIndex) + "\n" +
                            "> Subject: " + template.Subject + "\n" +
                            "> Content: " + template.Content + "\n" +
                            "> Attachments: " + template.Attachments + "\n" +
                            "\n" + "Press Esc To Return To Menu";

        return Temp;
    }

    public void AddCompleatedEvent(int EventIndex, int nameIndex)
    {
        //- Can do a max of -//
        RecorededActions[(nameIndex - 1), EventIndex] = new EventActivationInformation(EventIndex, nameIndex);
    }

    public int GetRandomFillerEmail()
    {
        int randomSelection = 0;
        do
        {
            randomSelection = Random.Range(0, FillerEmails.Length);
        }
        while (ActiveFillerEmails.Contains(randomSelection));
        ActiveFillerEmails.Add(randomSelection);

        return randomSelection;
    }

    public void ClearRandomSelectionMemory()
    {
        ActiveGeneratedEmails.Clear();
        ActiveFillerEmails.Clear();
    }

    public void ResetDay()
    {
        ActiveFillerEmails = new List<int>();
        ActiveGeneratedEmails = new List<int>();
        GenerateEmails();
    }
}
