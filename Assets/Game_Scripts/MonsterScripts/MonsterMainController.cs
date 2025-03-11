using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MonsterMainController : MonoBehaviour
{
    public enum MonsterTypes
    {
        skeleton,
        zombie,
    }

    public MonsterData[] MonsterDatas;
    public Texture[] MonsterMainTExtures;

    public MonsterData[][][] RarityDataByType;

    private void Start()
    {
        List<List<List<MonsterData>>> monsterDatasLists = new List<List<List<MonsterData>>>();
        for (int i = 0; i < Enum.GetValues(typeof(MonsterTypes)).Length; i++)
        {
            monsterDatasLists.Add(new List<List<MonsterData>>());
            for (int a = 0; a < Enum.GetValues(typeof(GlobalData.RarityDegree)).Length; a++)
            {
                monsterDatasLists[i].Add(new List<MonsterData>());
            }
        }


        foreach (MonsterData monsterData in MonsterDatas)
        {

            monsterDatasLists[(int)monsterData.MonsterTypes]        //First list
                [(int)monsterData.Rarity]                           //second list
                .Add(monsterData);                                  //finally data
        }
        RarityDataByType = monsterDatasLists
     .Select(list => list.Select(subList => subList.ToArray()).ToArray())
     .ToArray();

        List<Texture> textures = new List<Texture>();
        for (int i = 0; i < MonsterDatas.Length; i++)
        {
            MonsterDatas[i].monsterTextureNumber = i;
            textures.Add(MonsterDatas[i].monsterTexture);
        }
        MonsterMainTExtures = textures.ToArray();


    }



}

