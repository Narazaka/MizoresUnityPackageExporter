﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using YamlDotNet.Core.Tokens;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor {
#if UNITY_EDITOR
    public static class GUI_BatchExporter {
        static MizoresPackageExporter selected;
        static string selectedKey;
        static void Main( MizoresPackageExporter t, MizoresPackageExporter[] targetlist, bool samevalue_in_all_mode ) {
            bool multiple = targetlist.Length > 1;

            EditorGUI.BeginChangeCheck( );
            EditorGUI.showMixedValue = !samevalue_in_all_mode;
            using ( new EditorGUILayout.HorizontalScope( ) ) {
                ExporterUtils.Indent( 1 );
                t.batchExportMode = ( BatchExportMode )EditorGUILayout.EnumPopup( ExporterTexts.BatchExportMode, t.batchExportMode );
            }
            EditorGUI.showMixedValue = false;
            if ( EditorGUI.EndChangeCheck( ) ) {
                var mode = t.batchExportMode;
                foreach ( var item in targetlist ) {
                    item.batchExportMode = mode;
                    item.UpdateBatchExportKeys( );
                    EditorUtility.SetDirty( item );
                }
            }

            if ( samevalue_in_all_mode ) {
                switch ( t.batchExportMode ) {
                    default:
                    case BatchExportMode.Single:
                        break;
                    case BatchExportMode.Texts: {
                        var texts_count = MinMax.Create( targetlist, v => v.batchExportTexts.Count );
                        for ( int i = 0; i < texts_count.max; i++ ) {
                            using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                                // 全てのオブジェクトの値が同じか
                                bool samevalue_in_all = true;
                                if ( multiple ) {
                                    samevalue_in_all = i < texts_count.min && targetlist.All( v => t.batchExportTexts[i] == v.batchExportTexts[i] );
                                }

                                ExporterUtils.Indent( 2 );
                                if ( samevalue_in_all ) {
                                    EditorGUILayout.LabelField( i.ToString( ), GUILayout.Width( 30 ) );
                                } else {
                                    // 一部オブジェクトの値が異なっていたらTextFieldの左に?を表示
                                    ExporterUtils.DiffLabel( );
                                }

                                EditorGUI.BeginChangeCheck( );
                                Rect textrect = EditorGUILayout.GetControlRect( );
                                string path;
                                if ( samevalue_in_all ) {
                                    path = EditorGUI.TextField( textrect, t.batchExportTexts[i] );
                                } else {
                                    EditorGUI.showMixedValue = true;
                                    path = EditorGUI.TextField( textrect, string.Empty );
                                    EditorGUI.showMixedValue = false;
                                }

                                if ( EditorGUI.EndChangeCheck( ) ) {
                                    foreach ( var item in targetlist ) {
                                        ExporterUtils.ResizeList( item.batchExportTexts, Mathf.Max( i + 1, item.batchExportTexts.Count ) );
                                        item.batchExportTexts[i] = path;
                                        texts_count = MinMax.Create( targetlist, v => v.batchExportTexts.Count );
                                        item.UpdateBatchExportKeys( );
                                        EditorUtility.SetDirty( item );
                                    }
                                }

                                // Button
                                int index_after = ExporterUtils.UpDownButton( i, texts_count.max );
                                if ( i != index_after ) {
                                    foreach ( var item in targetlist ) {
                                        if ( item.batchExportTexts.Count <= index_after ) {
                                            ExporterUtils.ResizeList( item.batchExportTexts, index_after + 1 );
                                        }
                                        item.batchExportTexts.Swap( i, index_after );
                                        item.UpdateBatchExportKeys( );
                                        EditorUtility.SetDirty( item );
                                    }
                                }
                                EditorGUILayout.LabelField( string.Empty, GUILayout.Width( 10 ) );
                                if ( GUILayout.Button( "-", GUILayout.Width( 15 ) ) ) {
                                    foreach ( var item in targetlist ) {
                                        ExporterUtils.ResizeList( item.batchExportTexts, Mathf.Max( i + 1, item.batchExportTexts.Count ) );
                                        item.batchExportTexts.RemoveAt( i );
                                        texts_count = MinMax.Create( targetlist, v => v.batchExportTexts.Count );
                                        item.UpdateBatchExportKeys( );
                                        EditorUtility.SetDirty( item );
                                    }
                                    i--;
                                }
                            }
                        }
                        using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                            ExporterUtils.Indent( 1 );
                            if ( GUILayout.Button( "+", GUILayout.Width( 60 ) ) ) {
                                foreach ( var item in targetlist ) {
                                    ExporterUtils.ResizeList( item.batchExportTexts, texts_count.max + 1, ( ) => string.Empty );
                                    item.UpdateBatchExportKeys( );
                                    EditorUtility.SetDirty( item );
                                }
                            }
                        }
                        break;
                    }
                    case BatchExportMode.Folders: {
                        var samevalue_in_all_obj = targetlist.All( v => t.batchExportFolderRoot.Object == v.batchExportFolderRoot.Object );

                        if ( !samevalue_in_all_obj ) {
                            ExporterUtils.DiffLabel( );
                        }
                        EditorGUI.showMixedValue = !samevalue_in_all_obj;
                        EditorGUI.BeginChangeCheck( );
                        using ( new EditorGUILayout.HorizontalScope( ) ) {
                            ExporterUtils.Indent( 1 );
                            EditorGUILayout.LabelField( "Folder", GUILayout.Width( 60 ) );
                            PackagePrefsElementInspector.Draw<DefaultAsset>( t.batchExportFolderRoot );
                        }
                        EditorGUI.showMixedValue = false;
                        if ( EditorGUI.EndChangeCheck( ) ) {
                            var obj = t.batchExportFolderRoot.Object;
                            foreach ( var item in targetlist ) {
                                item.batchExportFolderRoot.Object = obj;
                                item.UpdateBatchExportKeys( );
                                EditorUtility.SetDirty( item );
                            }
                        }

                        var samevalue_in_all_regex = targetlist.All( v => t.batchExportFolderRegex == v.batchExportFolderRegex );
                        EditorGUI.BeginChangeCheck( );
                        EditorGUI.showMixedValue = !samevalue_in_all_regex;
                        using ( new EditorGUILayout.HorizontalScope( ) ) {
                            ExporterUtils.Indent( 1 );
                            t.batchExportFolderRegex = EditorGUILayout.TextField( ExporterTexts.BatchExportRegex, t.batchExportFolderRegex );
                        }
                        EditorGUI.showMixedValue = false;
                        if ( EditorGUI.EndChangeCheck( ) ) {
                            string regex = t.batchExportFolderRegex;
                            foreach ( var item in targetlist ) {
                                item.batchExportFolderRegex = regex;
                                item.UpdateBatchExportKeys( );
                                EditorUtility.SetDirty( item );
                            }
                        }
                        break;
                    }
                    case BatchExportMode.ListFile: {
                        var samevalue_in_all_obj = targetlist.All( v => t.batchExportListFile.Object == v.batchExportListFile.Object );

                        if ( !samevalue_in_all_obj ) {
                            ExporterUtils.DiffLabel( );
                        }
                        EditorGUI.showMixedValue = !samevalue_in_all_obj;
                        EditorGUI.BeginChangeCheck( );
                        using ( new EditorGUILayout.HorizontalScope( ) ) {
                            ExporterUtils.Indent( 1 );
                            EditorGUILayout.LabelField( "File", GUILayout.Width( 60 ) );
                            PackagePrefsElementInspector.Draw<TextAsset>( t.batchExportListFile );
                        }
                        EditorGUI.showMixedValue = false;
                        if ( EditorGUI.EndChangeCheck( ) ) {
                            var obj = t.batchExportListFile.Object;
                            foreach ( var item in targetlist ) {
                                item.batchExportListFile.Object = obj;
                                item.UpdateBatchExportKeys( );
                                EditorUtility.SetDirty( item );
                            }
                        }
                        break;
                    }
                }
            }
        }
        static void DrawList( MizoresPackageExporter[] targetlist ) {
            var t = targetlist[0];
            bool multiple = targetlist.Length > 1;
            bool first = true;
            foreach ( var item in targetlist ) {
                if ( item.batchExportMode == BatchExportMode.Single ) {
                    continue;
                }
                if ( first == false ) {
                    EditorGUILayout.Separator( );
                }
                first = false;
                if ( multiple ) {
                    using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                        GUI.enabled = false;
                        ExporterUtils.Indent( 1 );
                        EditorGUILayout.ObjectField( item, typeof( MizoresPackageExporter ), false );
                        GUI.enabled = true;
                    }
                }
                var list = item.BatchExportKeysConverted;
                for ( int i = 0; i < list.Length; i++ ) {
                    string key = list[i];
                    bool isSelected = false;
                    int indent = 1;
                    bool hasOverride = item.packageNameSettingsOverride.ContainsKey( key );
                    using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                        if ( multiple ) {
                            indent = 2;
                        }
                        ExporterUtils.Indent( indent );
                        Rect rect = EditorGUILayout.GetControlRect( );
                        var label = i.ToString( ) + "   " + key;
                        GUIStyle style;
                        if ( hasOverride ) {
                            style = EditorStyles.foldoutHeader;
                        } else {
                            style = EditorStyles.label;
                        }
                        isSelected = selected == item && selectedKey == key;
                        bool foldout = EditorGUI.BeginFoldoutHeaderGroup( rect, isSelected, label, style );
                        string buttonLabel;
                        if ( hasOverride ) {
                            buttonLabel = ExporterTexts.ButtonRemoveNameOverride;
                        } else {
                            buttonLabel = ExporterTexts.ButtonAddNameOverride;
                        }
                        if ( GUILayout.Button( buttonLabel, GUILayout.Width( 120 ) ) ) {
                            if ( hasOverride ) {
                                foldout = false;
                                item.packageNameSettingsOverride.Remove( key );
                                hasOverride = false;
                            } else {
                                foldout = true;
                                var settings = new PackageNameSettings( item.packageNameSettings );
                                item.packageNameSettingsOverride.Add( key, settings );
                                hasOverride = true;
                            }
                        }
                        if ( foldout && hasOverride ) {
                            selected = item;
                            selectedKey = key;
                            isSelected = true;
                        } else if ( isSelected ) {
                            selected = null;
                            selectedKey = null;
                            isSelected = false;
                        }
                        EditorGUI.EndFoldoutHeaderGroup( );
                    }
                    if ( isSelected && hasOverride ) {
                        var firstSettings = t.GetOverridedSettings(key);
                        var settingList = targetlist.Select(v=>v.GetOverridedSettings(key));

                        var same_useOverride_version_valueInAllObj = settingList.All( v => firstSettings.useOverride_version == v.useOverride_version );
                        using ( new EditorGUILayout.HorizontalScope( ) ) {
                            ExporterUtils.Indent( indent + 1 );
                            if ( !same_useOverride_version_valueInAllObj ) {
                                ExporterUtils.DiffLabel( );
                                EditorGUI.showMixedValue = true;
                            }
                            EditorGUI.BeginChangeCheck( );
                            var value = EditorGUILayout.Toggle( ExporterTexts.SettingOverrideVersion, firstSettings.useOverride_version );
                            EditorGUI.showMixedValue = false;
                            if ( EditorGUI.EndChangeCheck( ) ) {
                                foreach ( var s in settingList ) {
                                    s.useOverride_version = value;
                                    s.lastUpdate_ExportVersion = 0;
                                }
                                foreach ( var ta in targetlist ) {
                                    EditorUtility.SetDirty( ta );
                                }
                            }
                        }
                        if ( same_useOverride_version_valueInAllObj && firstSettings.useOverride_version ) {
                            var same_versionSource_valueInAllObj = settingList.All( v => firstSettings.versionSource == v.versionSource );
                            using ( new EditorGUILayout.HorizontalScope( ) ) {
                                ExporterUtils.Indent( indent );
                                EditorGUI.BeginChangeCheck( );
                                EditorGUI.showMixedValue = !same_versionSource_valueInAllObj;
                                var versionSource = ( VersionSource )EditorGUILayout.EnumPopup( ExporterTexts.VersionSource, firstSettings.versionSource );
                                EditorGUI.showMixedValue = false;
                                if ( EditorGUI.EndChangeCheck( ) ) {
                                    foreach ( var s in settingList ) {
                                        s.versionSource = versionSource;
                                        s.lastUpdate_ExportVersion = 0;
                                    }
                                    foreach ( var ta in targetlist ) {
                                        EditorUtility.SetDirty( ta );
                                    }
                                }
                            }
                            if ( same_versionSource_valueInAllObj ) {
                                using ( new EditorGUILayout.HorizontalScope( ) ) {
                                    ExporterUtils.Indent( indent );
                                    switch ( firstSettings.versionSource ) {
                                        case VersionSource.String: {
                                            var samevalue_in_all_obj = settingList.All( v => firstSettings.versionString == v.versionString );
                                            EditorGUI.BeginChangeCheck( );
                                            EditorGUI.showMixedValue = !samevalue_in_all_obj;
                                            string versionString = EditorGUILayout.TextField( ExporterTexts.Version, firstSettings.versionString );
                                            EditorGUI.showMixedValue = false;
                                            if ( EditorGUI.EndChangeCheck( ) ) {
                                                foreach ( var s in settingList ) {
                                                    s.versionString = versionString;
                                                    s.lastUpdate_ExportVersion = 0;
                                                }
                                                foreach ( var ta in targetlist ) {
                                                    EditorUtility.SetDirty( ta );
                                                }
                                            }
                                            break;
                                        }
                                        case VersionSource.File: {
                                            var samevalue_in_all_obj = settingList.All( v => firstSettings.versionFile.Object == v.versionFile.Object );

                                            if ( !samevalue_in_all_obj ) {
                                                ExporterUtils.DiffLabel( );
                                            }
                                            EditorGUI.showMixedValue = !samevalue_in_all_obj;
                                            EditorGUI.BeginChangeCheck( );
                                            PackagePrefsElementInspector.Draw<TextAsset>( firstSettings.versionFile );
                                            EditorGUI.showMixedValue = false;
                                            if ( EditorGUI.EndChangeCheck( ) ) {
                                                var obj = firstSettings.versionFile.Object;
                                                foreach ( var s in settingList ) {
                                                    s.versionFile.Object = obj;
                                                    s.lastUpdate_ExportVersion = 0;
                                                }
                                                foreach ( var ta in targetlist ) {
                                                    EditorUtility.SetDirty( ta );
                                                }
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        // Override Version Format
                        var same_useOverride_versionFormat_valueInAllObj = settingList.All( v => firstSettings.useOverride_versionFormat == v.useOverride_versionFormat );
                        using ( new EditorGUILayout.HorizontalScope( ) ) {
                            ExporterUtils.Indent( indent + 1 );
                            if ( !same_useOverride_versionFormat_valueInAllObj ) {
                                ExporterUtils.DiffLabel( );
                                EditorGUI.showMixedValue = true;
                            }
                            EditorGUI.BeginChangeCheck( );
                            var value = EditorGUILayout.Toggle( ExporterTexts.SettingOverrideVersionFormat, firstSettings.useOverride_versionFormat );
                            EditorGUI.showMixedValue = false;
                            if ( EditorGUI.EndChangeCheck( ) ) {
                                foreach ( var s in settingList ) {
                                    s.useOverride_versionFormat = value;
                                }
                                foreach ( var ta in targetlist ) {
                                    EditorUtility.SetDirty( ta );
                                }
                            }
                        }
                        if ( same_useOverride_versionFormat_valueInAllObj && firstSettings.useOverride_versionFormat ) {
                            // Version Format
                            using ( new EditorGUILayout.HorizontalScope( ) ) {
                                ExporterUtils.Indent( indent );
                                var samevalue_in_all = settingList.All( v => firstSettings.versionFormat == v.versionFormat );
                                EditorGUI.BeginChangeCheck( );
                                if ( !samevalue_in_all ) {
                                    ExporterUtils.DiffLabel( );
                                    EditorGUI.showMixedValue = true;
                                }
                                string value = EditorGUILayout.TextField( ExporterTexts.VersionFormat, firstSettings.versionFormat );
                                EditorGUI.showMixedValue = false;
                                if ( EditorGUI.EndChangeCheck( ) ) {
                                    foreach ( var s in settingList ) {
                                        s.versionFormat = value;
                                    }
                                    foreach ( var ta in targetlist ) {
                                        EditorUtility.SetDirty( ta );
                                    }
                                }
                            }
                        }
                        // Override Batch Format
                        var same_useOverride_batchFormat_valueInAllObj = settingList.All( v => firstSettings.useOverride_batchFormat == v.useOverride_batchFormat );
                        using ( new EditorGUILayout.HorizontalScope( ) ) {
                            ExporterUtils.Indent( indent + 1 );
                            if ( !same_useOverride_batchFormat_valueInAllObj ) {
                                ExporterUtils.DiffLabel( );
                                EditorGUI.showMixedValue = true;
                            }
                            EditorGUI.BeginChangeCheck( );
                            var value = EditorGUILayout.Toggle( ExporterTexts.SettingOverrideBatchFormat, firstSettings.useOverride_batchFormat );
                            EditorGUI.showMixedValue = false;
                            if ( EditorGUI.EndChangeCheck( ) ) {
                                foreach ( var s in settingList ) {
                                    s.useOverride_batchFormat = value;
                                }
                                foreach ( var ta in targetlist ) {
                                    EditorUtility.SetDirty( ta );
                                }
                            }
                        }
                        if ( same_useOverride_batchFormat_valueInAllObj && firstSettings.useOverride_batchFormat ) {
                            // Batch Format
                            using ( new EditorGUILayout.HorizontalScope( ) ) {
                                ExporterUtils.Indent( indent );
                                var samevalue_in_all = settingList.All( v => firstSettings.batchFormat == v.batchFormat );
                                EditorGUI.BeginChangeCheck( );
                                if ( !samevalue_in_all ) {
                                    ExporterUtils.DiffLabel( );
                                    EditorGUI.showMixedValue = true;
                                }
                                string value = EditorGUILayout.TextField( ExporterTexts.BatchFormat, firstSettings.batchFormat );
                                EditorGUI.showMixedValue = false;
                                if ( EditorGUI.EndChangeCheck( ) ) {
                                    foreach ( var s in settingList ) {
                                        s.batchFormat = value;
                                    }
                                    foreach ( var ta in targetlist ) {
                                        EditorUtility.SetDirty( ta );
                                    }
                                }
                            }
                        }
                        // Override Package Name
                        var same_useOverride_packageName_valueInAllObj = settingList.All( v => firstSettings.useOverride_packageName == v.useOverride_packageName );
                        using ( new EditorGUILayout.HorizontalScope( ) ) {
                            ExporterUtils.Indent( indent + 1 );
                            if ( !same_useOverride_packageName_valueInAllObj ) {
                                ExporterUtils.DiffLabel( );
                                EditorGUI.showMixedValue = true;
                            }
                            EditorGUI.BeginChangeCheck( );
                            var value = EditorGUILayout.Toggle( ExporterTexts.SettingOverridePackageName, firstSettings.useOverride_packageName );
                            EditorGUI.showMixedValue = false;
                            if ( EditorGUI.EndChangeCheck( ) ) {
                                foreach ( var s in settingList ) {
                                    s.useOverride_packageName = value;
                                }
                                foreach ( var ta in targetlist ) {
                                    EditorUtility.SetDirty( ta );
                                }
                            }
                        }
                        if ( same_useOverride_packageName_valueInAllObj && firstSettings.useOverride_packageName ) {
                            // Package Name
                            using ( new EditorGUILayout.HorizontalScope( ) ) {
                                ExporterUtils.Indent( indent );
                                var samevalue_in_all = settingList.All( v => firstSettings.packageName == v.packageName );
                                EditorGUI.BeginChangeCheck( );
                                if ( !samevalue_in_all ) {
                                    ExporterUtils.DiffLabel( );
                                    EditorGUI.showMixedValue = true;
                                }
                                string value = EditorGUILayout.TextField( ExporterTexts.PackageName, firstSettings.packageName );
                                EditorGUI.showMixedValue = false;
                                if ( EditorGUI.EndChangeCheck( ) ) {
                                    foreach ( var s in settingList ) {
                                        s.packageName = value;
                                    }
                                    foreach ( var ta in targetlist ) {
                                        EditorUtility.SetDirty( ta );
                                    }
                                }
                            }
                        }
                    }
                }

                using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                    ExporterUtils.Indent( 1 );
                    var unusedOverrides = item.packageNameSettingsOverride.Keys.Except( list );
                    EditorGUI.BeginDisabledGroup( !unusedOverrides.Any( ) );
                    if ( GUILayout.Button( ExporterTexts.ButtonCleanNameOverride ) ) {
                        foreach ( var remove in unusedOverrides ) {
                            Debug.Log( "Override Removed: \n" + remove );
                            item.packageNameSettingsOverride.Remove( remove );
                        }
                        Debug.Log( ExporterTexts.LogCleanNameOverride( unusedOverrides.Count( ) ) );
                    }
                    EditorGUI.EndDisabledGroup( );
                }
            }
        }
        public static void Draw( MizoresPackageExporterEditor ed, MizoresPackageExporter t, MizoresPackageExporter[] targetlist ) {
            var samevalue_in_all_mode = targetlist.All( v => t.batchExportMode == v.batchExportMode );
            string foldoutLabel;
            if ( samevalue_in_all_mode ) {
                if ( t.batchExportMode == BatchExportMode.Single ) {
                    foldoutLabel = ExporterTexts.FoldoutBatchExportDisabled;
                } else {
                    foldoutLabel = ExporterTexts.FoldoutBatchExportEnabled;
                }
            } else {
                foldoutLabel = ExporterTexts.FoldoutBatchExportEnabled;
            }
            if ( ExporterUtils.EditorPrefFoldout(
    ExporterEditorPrefs.FOLDOUT_EXPORT_SETTING, foldoutLabel ) ) {
                Main( t, targetlist, samevalue_in_all_mode );

                EditorGUILayout.Separator( );
                GUI_VersionFile.DrawMain( targetlist.Select( v => v.packageNameSettings ).ToArray( ), targetlist, 1 );

                if ( t.batchExportMode != BatchExportMode.Single ) {
                    ExporterUtils.SeparateLine( 1 );
                }
                DrawList( targetlist );
            }

            foreach ( var item in targetlist ) {
                if ( item.batchExportMode == BatchExportMode.Folders ) {
                    try {
                        Regex.Match( string.Empty, item.batchExportFolderRegex );
                    } catch ( System.ArgumentException e ) {
                        using ( new EditorGUILayout.HorizontalScope( ) ) {
                            var error = ExporterTexts.BatchExportRegexError( item.name, e.Message );
                            EditorGUILayout.HelpBox( error, MessageType.Error );
                        }
                    }
                }
                if ( item.batchExportMode != BatchExportMode.Single ) {
                    var packageName = item.packageNameSettings.packageName;
                    if ( !packageName.Contains( ExporterConsts_Keys.KEY_BATCH_EXPORTER ) && !packageName.Contains( ExporterConsts_Keys.KEY_FORMATTED_BATCH_EXPORTER ) ) {
                        var error = ExporterTexts.BatchExportNoTagError( item.name );
                        EditorGUILayout.HelpBox( error, MessageType.Error );
                    }
                }
            }
        }
    }
#endif
}