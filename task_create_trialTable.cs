using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class task_create_trialTable : MonoBehaviour
{
    public void create_trial_table()
    {

        TextAsset text_asset = (TextAsset)Resources.Load("Dirs_sub_" + task_main.subject_ID.ToString() + "_sess" +
                           task_main.subject_session.ToString());
        List<string> DataSet = TextAssetToList(text_asset);

        int number_of_trials = 120;
        task_main.reorganized_DataSet = new String[number_of_trials][];

        for (int i = 0; i < number_of_trials; i++)
        {
            string[] entries = DataSet[i].Split('\t');

            for (int j = 0; j < entries.Length; j++)
            {
                task_main.reorganized_DataSet[i] = entries;
            }
        }
    }

    private List<string> TextAssetToList(TextAsset ta)
    {
        return new List<string>(ta.text.Split('\n'));
    }


}
