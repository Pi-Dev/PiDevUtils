using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using static PiDev.Utils;

public class TestScript : MonoBehaviour
{
    public enum TestEnum { Entry1, Entry2, Entry3 }
    [Serializable] public struct TestStruct { public string name; public bool value; public int h; }
    public List<Weighted<string>> test1;
    public SerializableDictionary<Vector3, TestEnum> Dictionary;
    public SerializableDictionary<string, bool> DictionaryStringBool;
    public SerializableDictionary<string, Color> DictionaryStringColor;
    public SerializableDictionary<int, List<int>> DictOfStringToListInt;
    public SerializableDictionary<List<string>, TestEnum> DictOfListStringToEnum;
    public SerializableDictionary<GameObject, TestEnum> DictOfGameObjectToEnum;
    public SerializableDictionary<Transform, List<Transform>> DictTransformToTransforms;
    public SerializableDictionary<TestEnum, TestStruct> DictEnumToStruct;
    public SerializableDictionary<TestStruct, TestEnum> DictStructToEnum;
    public SerializableDictionary<TestStruct, SerializableDictionary<int, string> > DictOtTestToDicts;
    public SerializableDictionary<AudioClip, List<SerializableDictionary<string, string>>> DictHeavyNest;
    public bool something;
    public bool something2;
}
