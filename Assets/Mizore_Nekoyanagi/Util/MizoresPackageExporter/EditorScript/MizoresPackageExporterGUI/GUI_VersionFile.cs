﻿using UnityEngine;
using System.Linq;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor
{
#if UNITY_EDITOR
    public static class GUI_VersionFile
    {
        static void DrawMain( PackageNameSettings[] settings, MizoresPackageExporter[] targetlist ) {
            var t = targetlist[0];
            var s = settings[0];
            var same_versionSource_valueInAllObj = settings.All( v => s.versionSource == v.versionSource );
            using ( new EditorGUILayout.HorizontalScope( ) ) {
                ExporterUtils.Indent( 1 );
                EditorGUI.BeginChangeCheck( );
                VersionSource versionSource;

                EditorGUI.showMixedValue = !same_versionSource_valueInAllObj;
                versionSource = (VersionSource)EditorGUILayout.EnumPopup( ExporterTexts.VersionSource, s.versionSource );
                EditorGUI.showMixedValue = false;

                if ( EditorGUI.EndChangeCheck( ) ) {
                    for ( int i = 0; i < targetlist.Length; i++ ) {
                        settings[i].versionSource = versionSource;
                        targetlist[i].UpdateExportVersion( );
                        EditorUtility.SetDirty( targetlist[i] );
                    }
                }
            }
            if ( same_versionSource_valueInAllObj ) {
                using ( new EditorGUILayout.HorizontalScope( ) ) {
                    ExporterUtils.Indent( 1 );
                    switch ( s.versionSource ) {
                        case VersionSource.String: {
                            var samevalue_in_all_obj = settings.All( v => s.versionString == v.versionString );
                            EditorGUI.BeginChangeCheck( );
                            string versionString;

                            EditorGUI.showMixedValue = !samevalue_in_all_obj;
                            versionString = EditorGUILayout.TextField( ExporterTexts.Version, s.versionString );
                            EditorGUI.showMixedValue = false;

                            if ( EditorGUI.EndChangeCheck( ) ) {
                                for ( int i = 0; i < targetlist.Length; i++ ) {
                                    settings[i].versionString = versionString;
                                    targetlist[i].UpdateExportVersion( );
                                    EditorUtility.SetDirty( targetlist[i] );
                                }
                            }
                            break;
                        }
                        case VersionSource.File: {
                            var samevalue_in_all_obj = settings.All( v => s.versionFile.Object == v.versionFile.Object );

                            if ( !samevalue_in_all_obj ) {
                                ExporterUtils.DiffLabel( );
                            }
                            EditorGUI.showMixedValue = !samevalue_in_all_obj;
                            EditorGUI.BeginChangeCheck( );
                            PackagePrefsElementInspector.Draw<TextAsset>( s.versionFile );
                            EditorGUI.showMixedValue = false;
                            if ( EditorGUI.EndChangeCheck( ) ) {
                                var obj = s.versionFile.Object;
                                for ( int i = 0; i < targetlist.Length; i++ ) {
                                    settings[i].versionFile.Object = obj;
                                    targetlist[i].UpdateExportVersion( );
                                    EditorUtility.SetDirty( targetlist[i] );
                                }
                            }
                            break;
                        }
                    }
                }
            }

            // Version Format
            using ( new EditorGUILayout.HorizontalScope( ) ) {
                ExporterUtils.Indent( 1 );
                var samevalue_in_all = settings.All( v => s.versionFormat == v.versionFormat );
                EditorGUI.BeginChangeCheck( );
                string value;
                if ( !samevalue_in_all ) {
                    ExporterUtils.DiffLabel( );
                    EditorGUI.showMixedValue = true;
                }
                value = EditorGUILayout.TextField( ExporterTexts.VersionFormat, s.versionFormat );
                EditorGUI.showMixedValue = false;
                if ( EditorGUI.EndChangeCheck( ) ) {
                    for ( int i = 0; i < targetlist.Length; i++ ) {
                        settings[i].versionFormat = value;
                        // targetlist[i].UpdateExportVersion( );
                        EditorUtility.SetDirty( targetlist[i] );
                    }
                }
            }

            // Package Name
            using ( new EditorGUILayout.HorizontalScope( ) ) {
                ExporterUtils.Indent( 1 );
                var samevalue_in_all = settings.All( v => s.packageName == v.packageName );
                EditorGUI.BeginChangeCheck( );
                string value;
                if ( samevalue_in_all ) {
                    value = EditorGUILayout.TextField( ExporterTexts.PackageName, s.packageName );
                } else {
                    ExporterUtils.DiffLabel( );
                    EditorGUI.showMixedValue = true;
                    value = EditorGUILayout.TextField( ExporterTexts.PackageName, string.Empty );
                    EditorGUI.showMixedValue = false;
                }
                if ( EditorGUI.EndChangeCheck( ) ) {
                    for ( int i = 0; i < targetlist.Length; i++ ) {
                        settings[i].packageName = value;
                        targetlist[i].UpdateExportVersion( );
                        EditorUtility.SetDirty( targetlist[i] );
                    }
                }
            }
        }
        public static void Draw( MizoresPackageExporter t, MizoresPackageExporter[] targetlist ) {
            if ( ExporterUtils.EditorPrefFoldout(
                ExporterEditorPrefs.FOLDOUT_PACKAGE_NAME, ExporterTexts.FoldoutPackageName ) ) {
                DrawMain( targetlist.Select( v => v.packageNameSettings ).ToArray( ), targetlist );
            }
        }
    }
#endif
}
