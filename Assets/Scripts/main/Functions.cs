﻿using System;
using data;
using trial;
using UnityEngine;
using E = main.Loader;
using DS = data.DataSingleton;

namespace main
{    
    //Collection of classes that are called with reflection.
    //These are called at the end of the trail and are associated with the given pickup file.
    //Return true to repeat
    public class Functions
    {
        // ReSharper disable once UnusedMember.Global
        // Called with reflection
        public static bool CheckFoodThresholdPercentage(TrialProgress tp)
        {

            var curr = TrialProgress.GetCurrTrial();
            
            
            
            var blockId = curr.BlockID;


            var bd = DS.GetData().BlockList[blockId];

            var percentAndK = bd.EndGoal.Split(' ');

            var percent = float.Parse(percentAndK[0]);
            var num = float.Parse(percentAndK[1]);
            Debug.Log(string.Format(
                "TargetPercentage: {0}, Actual: {1}, NumTrial: {2}", 
                percent, tp.NumSuccess/tp.Num3D, tp.Num3D));
            
            
            
            return tp.NumSuccess / tp.Num3D < percent || tp.Num3D < num;
        }
    }
}