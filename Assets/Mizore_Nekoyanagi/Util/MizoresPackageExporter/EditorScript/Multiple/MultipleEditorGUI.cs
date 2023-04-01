﻿using UnityEngine;
using System.Linq;
using Const = MizoreNekoyanagi.PublishUtil.PackageExporter.MizoresPackageExporterConsts;
using static MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterUtils;
using MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor;
using System.Collections.Generic;
using MizoreNekoyanagi.PublishUtil.PackageExporter.SingleEditor;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.MultipleEditor
{

#if UNITY_EDITOR
    public static class MultipleEditorGUI
    {
        public static void AddObjects( IEnumerable<MizoresPackageExporter> targetlist, System.Func<MizoresPackageExporter, List<PackagePrefsElement>> getList, Object[] objectReferences ) {
            var add = objectReferences.
                Where( v => EditorUtility.IsPersistent( v ) ).
                Select( v => new PackagePrefsElement( v ) );
            foreach ( var item in targetlist ) {
                getList( item ).AddRange( add );
                EditorUtility.SetDirty( item );
            }
        }
        /// <summary>
        /// 複数オブジェクトの編集
        /// </summary>
        public static void EditMultiple( MizoresPackageExporterEditor ed ) {
            var targets = ed.targets;
            var t = ed.t;

            Undo.RecordObjects( targets, ExporterTexts.t_Undo );

            var targetlist = targets.Select( v => v as MizoresPackageExporter ).ToArray( );

            MizoresPackageExporter.debugmode = EditorGUILayout.Toggle( "Debug Mode", MizoresPackageExporter.debugmode );

            // Targets
            GUI.enabled = false;
            foreach ( var item in targetlist ) {
                EditorGUILayout.ObjectField( item, typeof( MizoresPackageExporter ), false );
            }
            GUI.enabled = true;

            ExporterUtils.SeparateLine( );

            // ↓ Objects
            MinMax objects_count = MinMax.Create( targetlist, v => v.objects.Count );
            if ( ExporterUtils.EditorPrefFoldout(
                Const.EDITOR_PREF_FOLDOUT_OBJECT,
                string.Format( ExporterTexts.t_Objects, objects_count.GetRangeString( ) ),
                new FoldoutFuncs( ) {
                    canDragDrop = objectReferences => objects_count.SameValue && ExporterUtils.Filter_HasPersistentObject( objectReferences ),
                    onDragPerform = ( objectReferences ) => AddObjects( targetlist, v => v.objects, objectReferences ),
                    onRightClick = ( ) => MultipleGUIElement_CopyPasteList.OnRightClickFoldout<PackagePrefsElement>( targetlist, ExporterTexts.t_Objects, ( ex ) => ex.objects, ( ex, list ) => ex.objects = list )
                }
                ) ) {
                MultipleGUIElement_PackagePrefsElementList.Draw<Object>( t, targetlist, ( v ) => v.objects );
            }
            // ↑ Objects

            // ↓ Dynamic Path
            GUI_DynamicPath.Draw( ed, t, targetlist );
            // ↑ Dynamic Path

            ExporterUtils.SeparateLine( );
            // ↓ References
            MinMax references_count = MinMax.Create( targetlist, v => v.references.Count );
            if ( ExporterUtils.EditorPrefFoldout(
                Const.EDITOR_PREF_FOLDOUT_REFERENCES,
                string.Format( ExporterTexts.t_References, references_count.GetRangeString( ) ),
                new FoldoutFuncs( ) {
                    canDragDrop = objectReferences => objects_count.SameValue && ExporterUtils.Filter_HasPersistentObject( objectReferences ),
                    onDragPerform = ( objectReferences ) => AddObjects( targetlist, v => v.references, objectReferences ),
                    onRightClick = ( ) => MultipleGUIElement_CopyPasteList.OnRightClickFoldout<PackagePrefsElement>( targetlist, ExporterTexts.t_References, ( ex ) => ex.references, ( ex, list ) => ex.references = list )
                }
                ) ) {
                MultipleGUIElement_PackagePrefsElementList.Draw<Object>( t, targetlist, ( v ) => v.references );
            }
            // ↑ References

            ExporterUtils.SeparateLine( );

            // ↓ Exclude Objects
            MinMax excludeObjects_count = MinMax.Create( targetlist, v => v.excludeObjects.Count );
            if ( ExporterUtils.EditorPrefFoldout(
                Const.EDITOR_PREF_FOLDOUT_EXCLUDE_OBJECTS,
                string.Format( ExporterTexts.t_ExcludeObjects, excludeObjects_count.GetRangeString( ) ),
                new FoldoutFuncs( ) {
                    canDragDrop = objectReferences => objects_count.SameValue && ExporterUtils.Filter_HasPersistentObject( objectReferences ),
                    onDragPerform = ( objectReferences ) => AddObjects( targetlist, v => v.excludeObjects, objectReferences ),
                    onRightClick = ( ) => MultipleGUIElement_CopyPasteList.OnRightClickFoldout<PackagePrefsElement>( targetlist, ExporterTexts.t_ExcludeObjects, ( ex ) => ex.excludeObjects, ( ex, list ) => ex.excludeObjects = list )
                }
                ) ) {
                MultipleGUIElement_PackagePrefsElementList.Draw<Object>( t, targetlist, ( v ) => v.excludeObjects );
            }
            // ↑ Exclude Objects

            // ↓ Excludes
            GUI_Excludes.Draw( ed, t, targetlist );
            // ↑ Excludes

            if ( targets.Length == 1 ) {
                ExporterUtils.SeparateLine( );
                // ↓ Dynamic Path Variables
                SingleGUI_DynamicPathVariables.Draw( ed, t );
                // ↑ Dynamic Path Variables
            }

            ExporterUtils.SeparateLine( );

            // ↓ Version File
            GUI_VersionFile.Draw( t, targetlist );
            // ↑ Version File

            ExporterUtils.SeparateLine( );

            // ExportPackage
            GUI_ExportPackage.Draw( ed, targetlist );
        }
    }
#endif
}
