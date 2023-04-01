﻿using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.MultipleEditor
{
    public static class MultipleGUIElement_CopyPasteList
    {
        static bool ListIsPerfectMatch<T>( IEnumerable<MizoresPackageExporter> targets, Func<MizoresPackageExporter, List<T>> getList ) {
            if ( targets.Count( ) == 1 ) {
                return true;
            }
            var t = getList( targets.First( ) );
            var lists = targets.Select( v => getList( v ) );
            foreach ( var item in lists ) {
                if ( t.Count != item.Count ) {
                    return false;
                }
                for ( int i = 0; i < t.Count; i++ ) {
                    if ( !t[i].Equals( item[i] ) ) {
                        return false;
                    }
                }
            }
            return true;
        }
        public static void OnRightClickFoldout<T>( IEnumerable<MizoresPackageExporter> targets, string labelFormat, Func<MizoresPackageExporter, List<T>> getList, Action<MizoresPackageExporter, List<T>> setList ) {
            GenericMenu menu = new GenericMenu( );
            var firstList = getList( targets.First( ) );
            bool perfectMatch = ListIsPerfectMatch( targets, getList );
            if ( perfectMatch ) {
                var copyLabel = string.Format( ExporterTexts.t_CopyTarget, string.Format( labelFormat, firstList.Count ) );
                menu.AddItem( new GUIContent( copyLabel ), false, CopyCache.Copy, firstList );
            } else {
                menu.AddDisabledItem( new GUIContent( ExporterTexts.t_CopyTargetNoValue ) );
            }
            if ( CopyCache.CanPaste<List<T>>( ) ) {
                var cache = CopyCache.GetCache<List<T>>( clone: false );
                string label = string.Format( ExporterTexts.t_PasteTarget, string.Format( labelFormat, cache.Count ) );
                menu.AddItem( new GUIContent( label ), false, ( ) => {
                    foreach ( var t in targets ) {
                        setList( t, CopyCache.GetCache<List<T>>( ) );
                        EditorUtility.SetDirty( t );
                    }
                } );
            } else {
                menu.AddDisabledItem( new GUIContent( ExporterTexts.t_PasteTargetNoValue ) );
            }
            menu.ShowAsContext( );
        }
    }
}
#endif
