using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace.utils;
using UnityEngine;

public class LoadCSV
{
    public List<double> data;
    private List<Dictionary<string, object>> _dataList;

    public LoadCSV(string path)
    {
        _dataList = CSVReader.Read(path);
        data = new List<double>();
    }

    public void Read(string column)
    {
        foreach (var _elem in _dataList)
        {
            Debug.Log(_elem[column]);
            double _temp = Convert.ToDouble(_elem[column]);
            data.Add(_temp);
        }
        
    }

    public static List<double> SyncTime(int length, List<double> target)
    {
        List<double> ret = new List<double>();
        
        double ratio = (double)target.Count / length;
        for (int i = 0; i < length; i++)
        {
            ret.Add(target[(int)Math.Floor(i * ratio)]);
        }

        return ret;
    }


}
