﻿using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor
{
#if UNITY_EDITOR
    [CustomEditor( typeof( MizoresPackageExporter ) ), CanEditMultipleObjects]
    public class MizoresPackageExporterEditor : Editor
    {
        public ExporterEditorLogs logs = new ExporterEditorLogs( );
        public string _variableKeyTemp;
        public MizoresPackageExporter t;

        private void OnEnable( ) {
            t = target as MizoresPackageExporter;
        }
        static string ToAssetsPath( string path ) {
            string datapath = Application.dataPath;
            if ( path.StartsWith( datapath ) ) {
                path = path.Substring( datapath.Length - "Assets".Length );
            }
            return path;
        }
        public string BrowseButtons( string text ) {
            string result = text;
            if ( GUILayout.Button( ExporterTexts.TEXT_BUTTON_FOLDER, GUILayout.Width( 50 ) ) ) {
                text = EditorUtility.OpenFolderPanel( null, t.ConvertDynamicPath( text ), null );
                text = ToAssetsPath( text );
                if ( string.IsNullOrEmpty( text ) == false ) {
                    GUI.changed = true;
                    result = text;
                }
            }
            if ( GUILayout.Button( ExporterTexts.TEXT_BUTTON_FILE, GUILayout.Width( 50 ) ) ) {
                text = EditorUtility.OpenFilePanel( null, t.ConvertDynamicPath( text ), null );
                text = ToAssetsPath( text );
                if ( string.IsNullOrEmpty( text ) == false ) {
                    GUI.changed = true;
                    result = text;
                }
            }
            return result;
        }
        public override void OnInspectorGUI( ) {
            foreach ( var item in targets ) {
                var exporter = item as MizoresPackageExporter;
                if ( !exporter.IsCompatible ) {
                    EditorGUILayout.HelpBox( string.Format( ExporterTexts.t_IncompatibleVersion, exporter.name ), MessageType.Error );
                    if ( GUILayout.Button( ExporterTexts.t_IncompatibleVersion_ForceOpen ) ) {
                        // SetDirtyはしない
                        exporter.ConvertToCurrentVersion( force: true );
                    }
                    return;
                }
                if ( !exporter.IsCurrentVersion ) {
                    exporter.ConvertToCurrentVersion( );
                    EditorUtility.SetDirty( exporter );
                }
            }
            MultipleEditor.MizoresPackageExporterEditorMain.EditMultiple( this );

            logs.DrawUI( );
        }
        public static void Export( ExporterEditorLogs logs, Object[] targets, PrePostPostProcessing action ) {
            var targetlist = targets.Select( v => v as MizoresPackageExporter ).ToArray( );
            Export( logs, targetlist, action );
        }
        public static void Export( ExporterEditorLogs logs, MizoresPackageExporter[] targets, PrePostPostProcessing action ) {
            logs.Clear( );
            for ( int i = 0; i < targets.Length; i++ ) {
                var item = targets[i];
                action?.export_preprocessing?.Invoke( item, i );
                item.Export( logs );
                action?.export_postprocessing?.Invoke( item, i );
            }
        }
        public void Export( ) {
            Export( logs, targets, null );
        }
    }
#endif
}
