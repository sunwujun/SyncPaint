using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Ink;

namespace SyncPaint
{
    sealed class CommandStack
    {
        public StrokeCollection strokeCollection;
        public  Stack<CommandItem> undoStack;
        public  Stack<CommandItem>  redoStack;

        bool _disableChangeTracking;
        public CommandStack(StrokeCollection strokes)
        {
            if (strokes == null)
            {
                throw new ArgumentNullException("strokes");
            }
            strokeCollection = strokes;
            undoStack = new Stack<CommandItem>();
            redoStack = new Stack<CommandItem>();
            _disableChangeTracking = false;
        }

        public StrokeCollection StrokeCollection
        {
            get
            {
                return strokeCollection;
            }
        }

        public bool CanUndo
        {
            get { return (undoStack.Count > 0); }
        }

        public bool CanRedo
        {
            get { return (redoStack.Count > 0); }
        }

        public void Undo()
        {
            if (!CanUndo) return;

            CommandItem item = undoStack.Pop();
            _disableChangeTracking = true;
            try
            {
                item.Undo();
            }
            finally
            {
                _disableChangeTracking = false;
            }
            redoStack.Push(item);
        }

        public void Redo()
        {
            if (!CanRedo) return;

            CommandItem item = redoStack.Pop();

            _disableChangeTracking = true;
            try
            {
                item.Redo();
            }
            finally
            {
                _disableChangeTracking = false;
            }

            undoStack.Push(item);
        }

        public void Enqueue(CommandItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (_disableChangeTracking)
            {
                return;
            }

            bool merged = false;
            if (undoStack.Count > 0)
            {
                CommandItem prev = undoStack.Peek();
                merged = prev.Merge(item);
            }

            if (!merged)
            {
                undoStack.Push(item);
            }

            if (redoStack.Count > 0)
            {
                redoStack.Clear();
            }
        }

    }

    abstract class CommandItem
    {

        public abstract void Undo();
        public abstract void Redo();

        public abstract bool Merge(CommandItem newitem);

        protected CommandStack _commandStack;

        protected CommandItem(CommandStack commandStack)
        {
            _commandStack = commandStack;
        }
    }

    class StrokesAddedOrRemovedCI : CommandItem
    {
        InkCanvasEditingMode _editingMode;
        StrokeCollection _added, _removed;
        int _editingOperationCount;

        public StrokesAddedOrRemovedCI(CommandStack commandStack, InkCanvasEditingMode editingMode, StrokeCollection added, StrokeCollection removed, int editingOperationCount)
            : base(commandStack)
        {
            _editingMode = editingMode;
            _added = added;
            _removed = removed;
            _editingOperationCount = editingOperationCount;
        }

        public override void Undo()
        {
            _commandStack.StrokeCollection.Remove(_added);
            _commandStack.StrokeCollection.Add(_removed);
        }

        public override void Redo()
        {
            _commandStack.StrokeCollection.Add(_added);
            _commandStack.StrokeCollection.Remove(_removed);
        }

        public override bool Merge(CommandItem newitem)
        {
            StrokesAddedOrRemovedCI newitemx = newitem as StrokesAddedOrRemovedCI;

            if (newitemx == null ||
                newitemx._editingMode != _editingMode ||
                newitemx._editingOperationCount != _editingOperationCount)
            {
                return false;
            }

            if (_editingMode != InkCanvasEditingMode.EraseByPoint) return false;
            if (newitemx._editingMode != InkCanvasEditingMode.EraseByPoint) return false;

            foreach (Stroke doomed in newitemx._removed)
            {
                if (_added.Contains(doomed))
                {
                    _added.Remove(doomed);
                }
                else
                {
                    _removed.Add(doomed);
                }
            }
            _added.Add(newitemx._added);
            return true;
        }
    }

    class SelectionMovedOrResizedCI : CommandItem
    {
        StrokeCollection _selection;
        Rect _newrect, _oldrect;
        int _editingOperationCount;

        public SelectionMovedOrResizedCI(CommandStack commandStack, StrokeCollection selection, Rect newrect, Rect oldrect, int editingOperationCount)
            : base(commandStack)
        {
            _selection = selection;
            _newrect = newrect;
            _oldrect = oldrect;
            _editingOperationCount = editingOperationCount;
        }

        public override void Undo()
        {
            Matrix m = GetTransformFromRectToRect(_newrect, _oldrect);
            _selection.Transform(m, false);
        }

        public override void Redo()
        {
            Matrix m = GetTransformFromRectToRect(_oldrect, _newrect);
            _selection.Transform(m, false);
        }

        public override bool Merge(CommandItem newitem)
        {
            SelectionMovedOrResizedCI newitemx = newitem as SelectionMovedOrResizedCI;

            if (newitemx == null ||
                newitemx._editingOperationCount != _editingOperationCount ||
                !StrokeCollectionsAreEqual(newitemx._selection, _selection))
            {
                return false;
            }

            _newrect = newitemx._newrect;

            return true;
        }

        static Matrix GetTransformFromRectToRect(Rect src, Rect dst)
        {
            Matrix m = Matrix.Identity;
            m.Translate(-src.X, -src.Y);
            m.Scale(dst.Width / src.Width, dst.Height / src.Height);
            m.Translate(+dst.X, +dst.Y);
            return m;
        }

        static bool StrokeCollectionsAreEqual(StrokeCollection a, StrokeCollection b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;
            if (a.Count != b.Count) return false;

            for (int i = 0; i < a.Count; ++i)
                if (a[i] != b[i]) return false;
            return true;
        }
    }
}
