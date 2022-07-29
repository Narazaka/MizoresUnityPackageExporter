﻿using UnityEngine;
using Const = MizoreNekoyanagi.PublishUtil.PackageExporter.MizoresPackageExporterConsts;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.MultipleEditor
{
#if UNITY_EDITOR
    public static class MultipleGUI_ExportPackage
    {
        public static void Draw( MizoresPackageExporterEditor ed, MizoresPackageExporter t, IEnumerable<MizoresPackageExporter> targetlist ) {
            EditorGUILayout.LabelField( ExporterTexts.t_Label_ExportPackage, EditorStyles.boldLabel );
            // Check Button
            if ( GUILayout.Button( ExporterTexts.t_Button_Check ) ) {
                ed.values._helpBoxText = string.Empty;
                foreach ( var item in targetlist ) {
                    item.AllFileExists( ed.values );
                }
            }

            // Export Button
            if ( GUILayout.Button( ExporterTexts.t_Button_ExportPackages ) ) {
                ed.values._helpBoxText = string.Empty;
                foreach ( var item in targetlist ) {
                    item.Export( ed.values );
                }
            }

            // 出力先一覧
            foreach ( var item in targetlist ) {
                var path = item.ExportPath;
                EditorGUILayout.LabelField( new GUIContent( path, path ) );
            }
            if ( GUILayout.Button( ExporterTexts.TEXT_BUTTON_OPEN, GUILayout.Width( 60 ) ) ) {
                EditorUtility.RevealInFinder( Const.EXPORT_FOLDER_PATH );
            }
        }
    }
#endif
}