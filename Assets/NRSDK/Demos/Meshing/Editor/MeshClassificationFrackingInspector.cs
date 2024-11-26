﻿/****************************************************************************
* Copyright 2019 Xreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.xreal.com/        
* 
*****************************************************************************/

using System.IO;
using UnityEditor;
using UnityEngine;

namespace NRKernal.NRExamples
{
#if UNITY_EDITOR
    [CustomEditor(typeof(MeshClassificationFracking))]
    public class MeshClassificationFrackingInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Load Obj Mesh File"))
            {
                string path = EditorUtility.OpenFolderPanel("Load Obj Mesh File", Application.dataPath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(path);
                    if (directoryInfo.Exists)
                    {
                        foreach (var file in directoryInfo.EnumerateFiles("*_with_label.obj"))
                        {
                            StreamReader sr = File.OpenText(file.FullName);
                            string meshData = sr.ReadToEnd();
                            sr.Close();
                            NRMeshInfo mesh = MeshSaveUtility.StringToMeshInfo(meshData);
                            (serializedObject.targetObject as IMeshInfoProcessor).UpdateMeshInfo(mesh.identifier, mesh);
                        }
                    }
                }
            }
        }

    }
#endif
}