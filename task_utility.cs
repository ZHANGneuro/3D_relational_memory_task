using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class task_utility
{


    public List<E> ShuffleList<E>(List<E> inputList)
    {
        List<E> randomList = new List<E>();
        System.Random r = new System.Random();
        int randomIndex = 0;
        while (inputList.Count > 0)
        {
            randomIndex = r.Next(0, inputList.Count); //Choose a random object in the list
            randomList.Add(inputList[randomIndex]); //add it to the new, random list
            inputList.RemoveAt(randomIndex); //remove to avoid duplicates
        }
        return randomList; //return the new random list
    }


    public Vector3 get_loc_1st_char1(float distance_camera_and_character, float reference_coordinate_x, float reference_coordinate_y, float reference_coordinate_z)
    {
        float k;
        float phi;
        float theta;
        List<float> list_3Dsphere_x = new List<float>();
        List<float> list_3Dsphere_y = new List<float>();
        List<float> list_3Dsphere_z = new List<float>();
        int num_attempt = 300;
        int random_num = UnityEngine.Random.Range(0, num_attempt);
        // k = random_num + .5f;
        // phi = Mathf.Acos(1f - 2f * k / num_attempt);
        // theta = Mathf.PI * (1 + Mathf.Sqrt(distance_camera_and_character)) * k;
        for (int ith_iter = 0; ith_iter < num_attempt; ith_iter++)
        {
            k = ith_iter + 0f;
            phi = Mathf.Acos(1f - 2f * k / num_attempt);
            theta = Mathf.PI * (1 + Mathf.Sqrt(distance_camera_and_character)) * k;
            float object0_x = Mathf.Cos(theta) * Mathf.Sin(phi) * distance_camera_and_character + reference_coordinate_x;
            float object0_y = Mathf.Sin(theta) * Mathf.Sin(phi) * distance_camera_and_character + reference_coordinate_y;
            float object0_z = Mathf.Cos(phi) * distance_camera_and_character + reference_coordinate_z;
            if (object0_y <= 23.0f && object0_y > 21.0f)
            {
                list_3Dsphere_x.Add(object0_x);
                list_3Dsphere_y.Add(object0_y);
                list_3Dsphere_z.Add(object0_z);
            }
            // list_3Dsphere_x.Add(object0_x);
            // list_3Dsphere_y.Add(object0_y);
            // list_3Dsphere_z.Add(object0_z);
        }
        int random_ith = UnityEngine.Random.Range(0, list_3Dsphere_x.Count);
        Vector3 character_originPos = new Vector3(list_3Dsphere_x[random_ith], list_3Dsphere_y[random_ith], list_3Dsphere_z[random_ith]);
        return character_originPos;
    }






}
