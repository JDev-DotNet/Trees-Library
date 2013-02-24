using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace JDev.Trees
{
    #region class AVLTreeOutputData<TKey, TValue>

    /// <summary>
    /// Class nhận dữ liệu từ cây
    /// </summary>
    /// <typeparam name="TKey">Kiểu dữ liệu của khóa của dữ liệu</typeparam>
    /// <typeparam name="TValue">Kiểu dữ liệu của dữ liệu</typeparam>
    [Serializable()]
    public class AVLTreeOutputData<TKey, TValue>
    {
        /// <summary>
        /// Khóa của dữ liệu
        /// </summary>
        public TKey Key;

        /// <summary>
        /// Dữ liệu
        /// </summary>
        public TValue Data;
    }

    #endregion

    #region class AVLTree<TKey, TValue>

    /// <summary>
    /// Cây nhị phân AVL
    /// </summary>
    /// <typeparam name="TKey">Kiểu dữ liệu của khóa</typeparam>
    /// <typeparam name="TValue">Kiểu dữ liệu của dữ liệu</typeparam>
    /// <remarks>Tốc độ tốt nhất so với các cây AVL còn lại</remarks>
    [Serializable()]
    public sealed class AVLTree<TKey, TValue> : IEnumerable<TValue>
    {
        #region Implement

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Trả về mảng dữ liệu trong cây
        /// </summary>
        /// <param name="visitMode">Kiểu duyệt cây</param>
        /// <returns>Mảng dữ liệu</returns>
        public AVLTreeOutputData<TKey, TValue>[] ToArray(VisitMode visitMode = VisitMode.LeftRootRight)
        {
            var dataArray = new AVLTreeOutputData<TKey, TValue>[Count()];

            var position = 0;

            if (visitMode == VisitMode.LeftRootRight)
            {
                LeftRootRightVisit(Root, ref dataArray, ref position);
            }

            if (visitMode == VisitMode.RightRootLeft)
            {
                RightRootLeftVisit(Root, ref dataArray, ref position);
            }

            if (visitMode == VisitMode.RootLeftRight)
            {
                RootLeftRightVisit(Root, ref dataArray, ref position);
            }

            return dataArray;
        }

        /// <summary>
        /// Xóa dữ liệu với khóa
        /// </summary>
        /// <param name="key">Khóa của dữ liệu cần xóa</param>
        /// <returns>Trả về true nếu thấy khóa và ngược lại</returns>
        public bool Delete(TKey key)
        {
            AVLNode node = Root;

            while (node != null)
            {
                if (_comparer.Compare(key, node.Key) < 0)
                {
                    node = node.Left;
                }
                else if (_comparer.Compare(key, node.Key) > 0)
                {
                    node = node.Right;
                }
                else
                {
                    AVLNode left = node.Left;
                    AVLNode right = node.Right;

                    if (left == null)
                    {
                        if (right == null)
                        {
                            if (node == Root)
                            {
                                Root = null;
                            }
                            else
                            {
                                AVLNode parent = node.Parent;

                                if (parent.Left == node)
                                {
                                    parent.Left = null;

                                    DeleteBalance(parent, -1);
                                }
                                else
                                {
                                    parent.Right = null;

                                    DeleteBalance(parent, 1);
                                }
                            }
                        }
                        else
                        {
                            Replace(node, right);

                            DeleteBalance(node, 0);
                        }
                    }
                    else if (right == null)
                    {
                        Replace(node, left);

                        DeleteBalance(node, 0);
                    }
                    else
                    {
                        AVLNode successor = right;

                        if (successor.Left == null)
                        {
                            AVLNode parent = node.Parent;

                            successor.Parent = parent;
                            successor.Left = left;
                            successor.Balance = node.Balance;

                            left.Parent = successor;

                            if (node == Root)
                            {
                                Root = successor;
                            }
                            else
                            {
                                if (parent.Left == node)
                                {
                                    parent.Left = successor;
                                }
                                else
                                {
                                    parent.Right = successor;
                                }
                            }

                            DeleteBalance(successor, 1);
                        }
                        else
                        {
                            while (successor.Left != null)
                            {
                                successor = successor.Left;
                            }

                            AVLNode parent = node.Parent;
                            AVLNode successorParent = successor.Parent;
                            AVLNode successorRight = successor.Right;

                            if (successorParent.Left == successor)
                            {
                                successorParent.Left = successorRight;
                            }
                            else
                            {
                                successorParent.Right = successorRight;
                            }

                            if (successorRight != null)
                            {
                                successorRight.Parent = successorParent;
                            }

                            successor.Parent = parent;
                            successor.Left = left;
                            successor.Balance = node.Balance;
                            successor.Right = right;
                            right.Parent = successor;

                            left.Parent = successor;

                            if (node == Root)
                            {
                                Root = successor;
                            }
                            else
                            {
                                if (parent.Left == node)
                                {
                                    parent.Left = successor;
                                }
                                else
                                {
                                    parent.Right = successor;
                                }
                            }

                            DeleteBalance(successorParent, -1);
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Khởi tạo với Interface so sánh
        /// </summary>
        /// <param name="comparer">Interface so sánh</param>
        public AVLTree(IComparer<TKey> comparer)
        {
            _comparer = comparer;
        }

        /// <summary>
        /// Khởi tạo mặc định (Khi kiểu dữ liệu đã kế thừa Interface IComparable)
        /// </summary>
        public AVLTree()
            : this(Comparer<TKey>.Default)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<TValue> GetEnumerator()
        {
            return new AVLNodeEnumerator(Root);
        }

        /// <summary>
        /// Xóa dữ liệu
        /// </summary>
        public void Clear()
        {
            Root = null;
        }

        /// <summary>
        /// Tìm kiếm theo khóa của dữ liệu
        /// </summary>
        /// <param name="key">Khóa của dữ liệu cần tìm</param>
        /// <param name="value">Dữ liệu lấy ra. Trả về null nếu không tìm thấy.</param>
        /// <returns>Trả về true nếu tìm được và ngược lại</returns>
        public bool Search(TKey key, out TValue value)
        {
            AVLNode node = Root;

            while (node != null)
            {
                if (_comparer.Compare(key, node.Key) < 0)
                {
                    node = node.Left;
                }
                else if (_comparer.Compare(key, node.Key) > 0)
                {
                    node = node.Right;
                }
                else
                {
                    value = node.Value;

                    return true;
                }
            }

            value = default(TValue);

            return false;
        }

        /// <summary>
        /// Tìm kiếm theo khóa của dữ liệu
        /// </summary>
        /// <param name="key">Khóa của dữ liệu</param>
        /// <returns>Dữ liệu lấy ra</returns>
        public TValue Search(TKey key)
        {
            TValue tValue = default(TValue);

            Search(key, out tValue);

            return tValue;
        }

        /// <summary>
        /// Thêm dữ liệu vào cây
        /// </summary>
        /// <param name="key">Khóa của dữ liệu</param>
        /// <param name="value">Dữ liệu</param>
        public void Add(TKey key, TValue value)
        {
            if (Root == null)
            {
                Root = new AVLNode { Key = key, Value = value };
            }
            else
            {
                AVLNode node = Root;

                while (node != null)
                {
                    int compare = _comparer.Compare(key, node.Key);

                    if (compare < 0)
                    {
                        AVLNode left = node.Left;

                        if (left == null)
                        {
                            node.Left = new AVLNode { Key = key, Value = value, Parent = node };

                            InsertBalance(node, 1);

                            return;
                        }
                        node = left;
                    }
                    else if (compare > 0)
                    {
                        AVLNode right = node.Right;

                        if (right == null)
                        {
                            node.Right = new AVLNode { Key = key, Value = value, Parent = node };

                            InsertBalance(node, -1);

                            return;
                        }
                        node = right;
                    }
                    else
                    {
                        node.Value = value;

                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Lấy số phần tử trong cây
        /// </summary>
        /// <returns>Số phần tử trong cây</returns>
        public int Count()
        {
            int count = 0;

            if (Root == null)
            {
                return count;
            }

            count++;

            Visit(Root.Left, ref count);

            Visit(Root.Right, ref count);

            return count;
        }

        /// <summary>
        /// Kiểm tra cây có dữ liệu hay không
        /// </summary>
        /// <returns>Trả về true nếu cây không có dữ liệu và ngược lại</returns>
        public bool IsEmpty()
        {
            return Root == null;
        }

        /// <summary>
        /// Tạo bản sao của cây
        /// </summary>
        /// <returns>Bản sao của cây</returns>
        public AVLTree<TKey, TValue> Clone()
        {
            return (AVLTree<TKey, TValue>)MemberwiseClone();
        }

        #endregion

        #region Private Methods

        private void Visit(AVLNode node, ref int count)
        {
            if (node == null)
            {
                return;
            }

            count++;

            Visit(node.Left, ref count);

            Visit(node.Right, ref count);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="balance"></param>
        private void InsertBalance(AVLNode node, int balance)
        {
            while (node != null)
            {
                balance = (node.Balance += balance);

                if (balance == 0)
                {
                    return;
                }
                if (balance == 2)
                {
                    if (node.Left.Balance == 1)
                    {
                        RotateRight(node);
                    }
                    else
                    {
                        RotateLeftRight(node);
                    }

                    return;
                }
                if (balance == -2)
                {
                    if (node.Right.Balance == -1)
                    {
                        RotateLeft(node);
                    }
                    else
                    {
                        RotateRightLeft(node);
                    }

                    return;
                }

                AVLNode parent = node.Parent;

                if (parent != null)
                {
                    balance = parent.Left == node ? 1 : -1;
                }

                node = parent;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="balance"></param>
        private void DeleteBalance(AVLNode node, int balance)
        {
            while (node != null)
            {
                balance = (node.Balance += balance);

                if (balance == 2)
                {
                    if (node.Left.Balance >= 0)
                    {
                        node = RotateRight(node);

                        if (node.Balance == -1)
                        {
                            return;
                        }
                    }
                    else
                    {
                        node = RotateLeftRight(node);
                    }
                }
                else if (balance == -2)
                {
                    if (node.Right.Balance <= 0)
                    {
                        node = RotateLeft(node);

                        if (node.Balance == 1)
                        {
                            return;
                        }
                    }
                    else
                    {
                        node = RotateRightLeft(node);
                    }
                }
                else if (balance != 0)
                {
                    return;
                }

                AVLNode parent = node.Parent;

                if (parent != null)
                {
                    balance = parent.Left == node ? -1 : 1;
                }

                node = parent;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private AVLNode RotateLeft(AVLNode node)
        {
            AVLNode right = node.Right;
            AVLNode rightLeft = right.Left;
            AVLNode parent = node.Parent;

            right.Parent = parent;
            right.Left = node;
            node.Right = rightLeft;
            node.Parent = right;

            if (rightLeft != null)
            {
                rightLeft.Parent = node;
            }

            if (node == Root)
            {
                Root = right;
            }
            else if (parent.Right == node)
            {
                parent.Right = right;
            }
            else
            {
                parent.Left = right;
            }

            right.Balance++;
            node.Balance = -right.Balance;

            return right;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private AVLNode RotateRight(AVLNode node)
        {
            AVLNode left = node.Left;
            AVLNode leftRight = left.Right;
            AVLNode parent = node.Parent;

            left.Parent = parent;
            left.Right = node;
            node.Left = leftRight;
            node.Parent = left;

            if (leftRight != null)
            {
                leftRight.Parent = node;
            }

            if (node == Root)
            {
                Root = left;
            }
            else if (parent.Left == node)
            {
                parent.Left = left;
            }
            else
            {
                parent.Right = left;
            }

            left.Balance--;
            node.Balance = -left.Balance;

            return left;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private AVLNode RotateLeftRight(AVLNode node)
        {
            AVLNode left = node.Left;
            AVLNode leftRight = left.Right;
            AVLNode parent = node.Parent;
            AVLNode leftRightRight = leftRight.Right;
            AVLNode leftRightLeft = leftRight.Left;

            leftRight.Parent = parent;
            node.Left = leftRightRight;
            left.Right = leftRightLeft;
            leftRight.Left = left;
            leftRight.Right = node;
            left.Parent = leftRight;
            node.Parent = leftRight;

            if (leftRightRight != null)
            {
                leftRightRight.Parent = node;
            }

            if (leftRightLeft != null)
            {
                leftRightLeft.Parent = left;
            }

            if (node == Root)
            {
                Root = leftRight;
            }
            else if (parent.Left == node)
            {
                parent.Left = leftRight;
            }
            else
            {
                parent.Right = leftRight;
            }

            if (leftRight.Balance == -1)
            {
                node.Balance = 0;
                left.Balance = 1;
            }
            else if (leftRight.Balance == 0)
            {
                node.Balance = 0;
                left.Balance = 0;
            }
            else
            {
                node.Balance = -1;
                left.Balance = 0;
            }

            leftRight.Balance = 0;

            return leftRight;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private AVLNode RotateRightLeft(AVLNode node)
        {
            AVLNode right = node.Right;
            AVLNode rightLeft = right.Left;
            AVLNode parent = node.Parent;
            AVLNode rightLeftLeft = rightLeft.Left;
            AVLNode rightLeftRight = rightLeft.Right;

            rightLeft.Parent = parent;
            node.Right = rightLeftLeft;
            right.Left = rightLeftRight;
            rightLeft.Right = right;
            rightLeft.Left = node;
            right.Parent = rightLeft;
            node.Parent = rightLeft;

            if (rightLeftLeft != null)
            {
                rightLeftLeft.Parent = node;
            }

            if (rightLeftRight != null)
            {
                rightLeftRight.Parent = right;
            }

            if (node == Root)
            {
                Root = rightLeft;
            }
            else if (parent.Right == node)
            {
                parent.Right = rightLeft;
            }
            else
            {
                parent.Left = rightLeft;
            }

            if (rightLeft.Balance == 1)
            {
                node.Balance = 0;
                right.Balance = -1;
            }
            else if (rightLeft.Balance == 0)
            {
                node.Balance = 0;
                right.Balance = 0;
            }
            else
            {
                node.Balance = 1;
                right.Balance = 0;
            }

            rightLeft.Balance = 0;

            return rightLeft;
        }

        /// <summary>
        /// 
        /// </summary>
        private IComparer<TKey> _comparer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="source"></param>
        private static void Replace(AVLNode target, AVLNode source)
        {
            AVLNode left = source.Left;
            AVLNode right = source.Right;

            target.Balance = source.Balance;
            target.Key = source.Key;
            target.Value = source.Value;
            target.Left = left;
            target.Right = right;

            if (left != null)
            {
                left.Parent = target;
            }

            if (right != null)
            {
                right.Parent = target;
            }
        }

        private void LeftRootRightVisit(AVLNode node, ref AVLTreeOutputData<TKey, TValue>[] dataArray, ref int position)
        {
            if (node == null)
            {
                return;
            }

            if (node.Left != null)
            {
                LeftRootRightVisit(node.Left, ref dataArray, ref position);
                //dataArray[position++] = node.Data;
            }

            dataArray[position] = new AVLTreeOutputData<TKey, TValue> { Data = node.Value, Key = node.Key };
            position++;

            if (node.Right != null)
            {
                LeftRootRightVisit(node.Right, ref dataArray, ref position);
                //dataArray[position++] = node.Data;  
            }
        }

        private void RightRootLeftVisit(AVLNode node, ref AVLTreeOutputData<TKey, TValue>[] dataArray, ref int position)
        {
            if (node == null)
            {
                return;
            }

            if (node.Right != null)
            {
                RightRootLeftVisit(node.Right, ref dataArray, ref position);
                //dataArray[position++] = node.Data;  
            }

            dataArray[position] = new AVLTreeOutputData<TKey, TValue> { Data = node.Value, Key = node.Key };
            position++;

            if (node.Left != null)
            {
                RightRootLeftVisit(node.Left, ref dataArray, ref position);
                //dataArray[position++] = node.Data;
            }
        }

        private void RootLeftRightVisit(AVLNode node, ref AVLTreeOutputData<TKey, TValue>[] dataArray, ref int position)
        {
            if (node == null)
            {
                return;
            }

            dataArray[position] = new AVLTreeOutputData<TKey, TValue> { Data = node.Value, Key = node.Key };
            position++;

            if (node.Left != null)
            {
                RootLeftRightVisit(node.Left, ref dataArray, ref position);
                //dataArray[position++] = node.Data;
            }

            if (node.Right != null)
            {
                RootLeftRightVisit(node.Right, ref dataArray, ref position);
                //dataArray[position++] = node.Data;  
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Lấy, gán Interface so sánh
        /// </summary>
        public IComparer<TKey> Comparer
        {
            set { _comparer = value; }
            get { return _comparer; }
        }

        #endregion

        #region Fields

        /// <summary>
        /// 
        /// </summary>
        private AVLNode Root;

        #endregion

        #region class AVLNode

        /// <summary>
        /// 
        /// </summary>
        private sealed class AVLNode
        {
            public AVLNode Parent;
            public AVLNode Left;
            public AVLNode Right;
            public TKey Key;
            public TValue Value;
            public int Balance;
        }

        #endregion

        #region class AVLNodeEnumerator

        /// <summary>
        /// 
        /// </summary>
        private sealed class AVLNodeEnumerator : IEnumerator<TValue>
        {
            private readonly AVLNode _root;
            private Action _action;
            private AVLNode _current;
            private AVLNode _right;

            #region Public Methods

            /// <summary>
            /// 
            /// </summary>
            /// <param name="root"></param>
            public AVLNodeEnumerator(AVLNode root)
            {
                _right = _root = root;

                _action = root == null ? Action.End : Action.Right;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public bool MoveNext()
            {
                switch (_action)
                {
                    case Action.Right:
                        _current = _right;

                        while (_current.Left != null)
                        {
                            _current = _current.Left;
                        }

                        _right = _current.Right;

                        _action = _right != null ? Action.Right : Action.Parent;

                        return true;
                    case Action.Parent:
                        while (_current.Parent != null)
                        {
                            AVLNode previous = _current;

                            _current = _current.Parent;

                            if (_current.Left == previous)
                            {
                                _right = _current.Right;

                                _action = _right != null ? Action.Right : Action.Parent;

                                return true;
                            }
                        }

                        _action = Action.End;

                        return false;
                    default:
                        return false;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public void Reset()
            {
                _right = _root;

                _action = _root == null ? Action.End : Action.Right;
            }

            /// <summary>
            /// 
            /// </summary>
            public void Dispose()
            {
            }

            #endregion

            #region Properties

            /// <summary>
            /// 
            /// </summary>
            public TValue Current
            {
                get { return _current.Value; }
            }

            /// <summary>
            /// 
            /// </summary>
            object IEnumerator.Current
            {
                get { return Current; }
            }

            #endregion

            #region Enums

            /// <summary>
            /// 
            /// </summary>
            private enum Action
            {
                Parent,
                Right,
                End
            }

            #endregion
        }

        #endregion
    }

    #endregion

    #region class AVLTree<TValue>

    /// <summary>
    /// Cây nhị phân AVL
    /// </summary>
    /// <typeparam name="TValue">Kiểu dữ liệu của dữ liệu</typeparam>
    [Serializable()]
    public sealed class AVLTree<TValue>
    {
        #region Fields

        /// <summary>
        /// Gốc
        /// </summary>
        private Node<TValue> Root;

        /// <summary>
        /// Interface so sánh
        /// </summary>
        private IComparer<TValue> comparer;

        #endregion

        #region Ctor

        /// <summary>
        /// Khởi tạo mặc định. Dữ liệu đã kế thừa Interface IComparable.
        /// </summary>
        public AVLTree()
        {
            comparer = GetComparer();
        }

        /// <summary>
        /// Khởi tạo với dữ liệu có sẵn.
        /// </summary>
        /// <param name="elems">Dữ liệu có sẵn (có thể null)</param>
        /// <param name="comparer">Interface so sánh</param>
        public AVLTree(IEnumerable<TValue> elems, IComparer<TValue> comparer)
        {
            this.comparer = comparer;

            if (elems != null)
            {
                foreach (var elem in elems)
                {
                    Add(elem);
                }
            }
        }

        #endregion

        #region Delegates

        /// <summary>
        /// the visitor delegate
        /// </summary>
        /// <typeparam name="TNode">Kiểu dữ liệu của một Node</typeparam>
        /// <param name="node">Dữ liệu</param>
        /// <param name="level">Cấp độ</param>
        public delegate void VisitNodeHandler<in TNode>(TNode node, int level);

        #endregion

        #region Enums

        /// <summary>
        /// 
        /// </summary>
        public enum SplitOperationMode
        {
            /// <summary>
            /// 
            /// </summary>
            IncludeSplitValueToLeftSubtree,

            /// <summary>
            /// 
            /// </summary>
            IncludeSplitValueToRightSubtree,

            /// <summary>
            /// 
            /// </summary>
            DoNotIncludeSplitValue
        }

        #endregion

        #region Properties

        /// <summary>
        /// Lấy, gán Interface so sánh
        /// </summary>
        public IComparer<TValue> Comparer
        {
            set { comparer = value; }
            get { return comparer; }
        }

#if TREE_WITH_PARENT_POINTERS

    /// <summary>
    /// Gets the collection of values in ascending order. 
    /// Complexity: O(N)
    /// </summary>
        public IEnumerable<T> ValuesCollection
        {
            get
            {
                if (this.Root == null)
                {
                    yield break;
                }

                var p = FindMin(this.Root);
                while (p != null)
                {
                    yield return p.Data;
                    p = Successor(p);
                }
            }
        }

        /// <summary>
        /// Gets the collection of values in reverse/descending order. 
        /// Complexity: O(N)
        /// </summary>
        public IEnumerable<T> ValuesCollectionDescending
        {
            get
            {
                if (this.Root == null)
                {
                    yield break;
                }

                var p = FindMax(this.Root);
                while (p != null)
                {
                    yield return p.Data;
                    p = Predecesor(p);
                }
            }
        }

#endif

        #endregion

        #region Public Methods

        /// <summary>
        /// Thêm dữ liệu vào cây.
        /// </summary>
        /// <param name="arg">Dữ liệu</param>
        /// <returns>Trả về true nếu thêm thành công và ngược lại</returns>
        public bool Add(TValue arg)
        {
            bool wasAdded = false;
            bool wasSuccessful = false;

            Root = Add(Root, arg, ref wasAdded, ref wasSuccessful);

            return wasSuccessful;
        }

        /// <summary>
        /// Xóa dữ liệu ra khỏi cây
        /// </summary>
        /// <param name="arg">Dữ liệu</param>
        /// <returns>Trả về true nếu xóa thành công và ngược lại</returns>
        public bool Delete(TValue arg)
        {
            var wasSuccessful = false;

            if (Root != null)
            {
                bool wasDeleted = false;
                Root = Delete(Root, arg, ref wasDeleted, ref wasSuccessful);
            }

            return wasSuccessful;
        }

        /// <summary>
        /// Lấy dữ liệu có giá trị nhỏ nhất trong cây
        /// </summary>
        /// <param name="value">Dữ liệu lấy ra</param>
        /// <returns>Trả về true nếu lấy thành công và ngược lại</returns>
        public bool GetMin(out TValue value)
        {
            if (Root != null)
            {
                var min = FindMin(Root);
                if (min != null)
                {
                    value = min.Data;
                    return true;
                }
            }

            value = default(TValue);
            return false;
        }

        /// <summary>
        /// Lấy dữ liệu có giá trị lớn nhất trong cây
        /// </summary>
        /// <param name="value">Dữ liệu lấy ra</param>
        /// <returns>Trả về true nếu lấy thành công và ngược lại</returns>
        public bool GetMax(out TValue value)
        {
            if (Root != null)
            {
                var max = FindMax(Root);
                if (max != null)
                {
                    value = max.Data;
                    return true;
                }
            }

            value = default(TValue);
            return false;
        }

        /// <summary>
        /// Kiểm tra dữ liệu có nằm trong cây không
        /// </summary>
        /// <param name="arg">Dữ liệu cần kiểm tra</param>
        /// <returns> Trả về true nếu tìm thấy và ngược lại</returns>
        public bool Exists(TValue arg)
        {
            return Search(Root, arg) != null;
        }

        /// <summary>
        /// Xóa dữ liệu có giá trị nhỏ nhất trong cây
        /// </summary>
        /// <returns>Trả về true nếu xóa thành công và ngược lại</returns>
        public bool DeleteMin()
        {
            if (Root != null)
            {
                bool wasDeleted = false, wasSuccessful = false;
                Root = DeleteMin(Root, ref wasDeleted, ref wasSuccessful);

                return wasSuccessful;
            }

            return false;
        }

        /// <summary>
        /// Xóa dữ liệu có giá trị lớn nhất trong cây
        /// </summary>
        /// <returns>Trả về true nếu xóa thành công và ngược lại</returns>
        public bool DeleteMax()
        {
            if (Root != null)
            {
                bool wasDeleted = false, wasSuccessful = false;
                Root = DeleteMax(Root, ref wasDeleted, ref wasSuccessful);

                return wasSuccessful;
            }

            return false;
        }

        /// <summary>
        /// Tìm kiếm dữ liệu trong cây
        /// </summary>
        /// <param name="arg">Dữ liệu cần tìm</param>
        /// <returns>Dữ liệu lấy ra</returns>
        /// <remarks>Sự thay đổi dữ liệu có thể phá vỡ tính đúng đắn của cây</remarks>
        public TValue Search(TValue arg)
        {
            Node<TValue> temp = Search(Root, arg);
            if (temp != null)
            {
                return temp.Data;
            }
            return default(TValue);
        }

        /// <summary>
        /// Tạo một bản sao của cây
        /// </summary>
        /// <returns>Bản sao của cây</returns>
        public AVLTree<TValue> Clone()
        {
            return (AVLTree<TValue>)MemberwiseClone();
        }

        /// <summary>
        /// Trả về mảng dữ liệu trong cây
        /// </summary>
        /// <param name="visitMode">Kiểu duyệt cây</param>
        /// <returns>Mảng dữ liệu</returns>
        public TValue[] ToArray(VisitMode visitMode = VisitMode.LeftRootRight)
        {
            var dataArray = new TValue[Count()];

            var position = 0;

            if (visitMode == VisitMode.LeftRootRight)
            {
                LeftRootRightVisit(Root, ref dataArray, ref position);
            }

            if (visitMode == VisitMode.RightRootLeft)
            {
                RightRootLeftVisit(Root, ref dataArray, ref position);
            }

            if (visitMode == VisitMode.RootLeftRight)
            {
                RootLeftRightVisit(Root, ref dataArray, ref position);
            }

            return dataArray;
        }

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS

    /// <summary>
    /// Concatenates the elements of the two trees. 
    /// Precondition: ALL elements of the 'other' argument AVL tree must be LARGER than all elements contained in this instance.
    /// Complexity: O(log(N1) + log(N2)). See Remarks section below.
    /// </summary>
    /// <remarks>
    /// Complexity: 
    ///     Assuming height(node1) > height(node2), our procedure runs in height(node1) + height(node2) i.e. O(log(n1)) + O(log(n2)) due to the two calls to findMin/deleteMin (or findMax, deleteMax respectively). 
    ///     Runs in O(height(node1)) if height(node1) == height(node2).
    /// Improvements:
    ///     Performing find/delete in one operation gives O(height(node1)) speed.
    ///     Furthermore, if storing min value for each subtree, one obtains the theoretical O(height(node1) - height(node2)). 
    /// </remarks>
        public AVLTree<T> Concat(AVLTree<T> other)
        {
            if (other == null)
            {
                return this;
            }

            var root = Concat(this.Root, other.Root);
            if (root != null)
            {
                return new AVLTree<T>() { Root = root };
            }

            return null;
        }

        /// <summary>
        /// Splits this AVL Tree instance into 2 AVL subtrees using the specified value as the cut/split point.
        /// The value to split by must exist in the tree.
        /// This function is destructive (i.e. the current AVL tree instance is not a valid anymore upon return of this function)
        /// </summary>
        /// <param name="value">The value to use when splitting this instance.</param>
        /// <param name="mode">The mode specifying what to do with the value used for splitting. Options are not to include this value in either of the two resulting trees, to include it in the left or to include it in the right tree respectively</param>
        /// <param name="splitLeftTree">[out] The left avl tree. Upon return, all values of this subtree are less than the value argument.</param>
        /// <param name="splitRightTree">[out] The right avl tree. Upon return, all values of this subtree are greater than the value argument.</param>
        /// <returns>a boolean indicating success or failure</returns>
        public bool Split(T value, SplitOperationMode mode, out AVLTree<T> splitLeftTree, out AVLTree<T> splitRightTree)
        {
            splitLeftTree = null;
            splitRightTree = null;

            Node<T> splitLeftRoot = null, splitRightRoot = null;
            bool wasFound = false;

            this.Split(this.Root, value, ref splitLeftRoot, ref splitRightRoot, mode, ref wasFound);
            if (wasFound)
            {
                splitLeftTree = new AVLTree<T>() { Root = splitLeftRoot };
                splitRightTree = new AVLTree<T>() { Root = splitRightRoot };
            }

            return wasFound;
        }

#endif

        /// <summary>
        /// Lấy độ cao của cây
        /// </summary>
        /// <returns>Độ cao của cây</returns>
        public int GetHeight()
        {
            return GetHeight(Root);
        }

        /// <summary>
        /// Xóa toàn bộ dữ liệu trong cây
        /// </summary>
        public void Clear()
        {
            Root = null;
        }

        /// <summary>
        /// Lấy số phần tử trong cây
        /// </summary>
        /// <returns>Số phần tử trong cây</returns>
        public int Count()
        {
            int count = 0;
            Visit((node, level) => { count++; });
            return count;
        }

        /// <summary>
        /// Kiểm tra cây có dữ liệu hay không
        /// </summary>
        /// <returns>Trả về true nếu cây không có dữ liệu và ngược lại</returns>
        public bool IsEmpty()
        {
            return Root == null;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static IComparer<TValue> GetComparer()
        {
            if (typeof(IComparable<TValue>).IsAssignableFrom(typeof(TValue)) ||
                typeof(IComparable).IsAssignableFrom(typeof(TValue)))
            {
                return Comparer<TValue>.Default;
            }
            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                                                              "The type {0} cannot be compared. It must implement IComparable<T> or IComparable interface",
                                                              typeof(TValue).FullName));
        }

        /// <summary>
        /// Gets the height of the tree in log(n) time.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The height of the tree. Runs in O(log(n)) where n is the number of nodes in the tree </returns>
        private int GetHeight(Node<TValue> node)
        {
            if (node == null)
            {
                return 0;
            }
            int leftHeight = GetHeight(node.Left);
            if (node.Balance == 1)
            {
                leftHeight++;
            }

            return 1 + leftHeight;
        }

        /// <summary>
        /// Adds the specified data to the tree identified by the specified argument.
        /// </summary>
        /// <param name="elem">The elem.</param>
        /// <param name="data">The data.</param>
        /// <param name="wasAdded"> </param>
        /// <param name="wasSuccessful"> </param>
        /// <returns></returns>
        private Node<TValue> Add(Node<TValue> elem, TValue data, ref bool wasAdded, ref bool wasSuccessful)
        {
            if (elem == null)
            {
                elem = new Node<TValue> { Data = data, Left = null, Right = null, Balance = 0 };

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS
                elem.Height = 1;
#endif

                wasAdded = true;
                wasSuccessful = true;
            }
            else
            {
                int resultCompare = comparer.Compare(data, elem.Data);

                if (resultCompare < 0)
                {
                    var newLeft = Add(elem.Left, data, ref wasAdded, ref wasSuccessful);
                    if (elem.Left != newLeft)
                    {
                        elem.Left = newLeft;
#if TREE_WITH_PARENT_POINTERS
                        newLeft.Parent = elem;
#endif
                    }

                    if (wasAdded)
                    {
                        --elem.Balance;

                        if (elem.Balance == 0)
                        {
                            wasAdded = false;
                        }
                        else if (elem.Balance == -2)
                        {
                            int leftBalance = newLeft.Balance;
                            if (leftBalance == 1)
                            {
                                int elemLeftRightBalance = newLeft.Right.Balance;

                                elem.Left = RotateLeft(newLeft);
                                elem = RotateRight(elem);

                                elem.Balance = 0;
                                elem.Left.Balance = elemLeftRightBalance == 1 ? -1 : 0;
                                elem.Right.Balance = elemLeftRightBalance == -1 ? 1 : 0;
                            }
                            else if (leftBalance == -1)
                            {
                                elem = RotateRight(elem);
                                elem.Balance = 0;
                                elem.Right.Balance = 0;
                            }

                            wasAdded = false;
                        }
                    }
                }
                else if (resultCompare > 0)
                {
                    var newRight = Add(elem.Right, data, ref wasAdded, ref wasSuccessful);
                    if (elem.Right != newRight)
                    {
                        elem.Right = newRight;
#if TREE_WITH_PARENT_POINTERS
                        newRight.Parent = elem;
#endif
                    }

                    if (wasAdded)
                    {
                        ++elem.Balance;
                        if (elem.Balance == 0)
                        {
                            wasAdded = false;
                        }
                        else if (elem.Balance == 2)
                        {
                            int rightBalance = newRight.Balance;
                            if (rightBalance == -1)
                            {
                                int elemRightLeftBalance = newRight.Left.Balance;

                                elem.Right = RotateRight(newRight);
                                elem = RotateLeft(elem);

                                elem.Balance = 0;
                                elem.Left.Balance = elemRightLeftBalance == 1 ? -1 : 0;
                                elem.Right.Balance = elemRightLeftBalance == -1 ? 1 : 0;
                            }
                            else if (rightBalance == 1)
                            {
                                elem = RotateLeft(elem);

                                elem.Balance = 0;
                                elem.Left.Balance = 0;
                            }

                            wasAdded = false;
                        }
                    }
                }

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS
                elem.Height = 1 + Math.Max(
                                        elem.Left != null ? elem.Left.Height : 0,
                                        elem.Right != null ? elem.Right.Height : 0);
#endif
            }

            return elem;
        }

        /// <summary>
        /// Deletes the specified arg. value from the tree.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="arg">The arg.</param>
        /// <param name="wasDeleted"> </param>
        /// <param name="wasSuccessful"> </param>
        /// <returns></returns>
        private Node<TValue> Delete(Node<TValue> node, TValue arg, ref bool wasDeleted, ref bool wasSuccessful)
        {
            int cmp = comparer.Compare(arg, node.Data);
            Node<TValue> newChild = null;

            if (cmp < 0)
            {
                if (node.Left != null)
                {
                    newChild = Delete(node.Left, arg, ref wasDeleted, ref wasSuccessful);
                    if (node.Left != newChild)
                    {
                        node.Left = newChild;
                    }

                    if (wasDeleted)
                    {
                        node.Balance++;
                    }
                }
            }
            else if (cmp == 0)
            {
                wasDeleted = true;
                if (node.Left != null && node.Right != null)
                {
                    var min = FindMin(node.Right);
                    TValue data = node.Data;
                    node.Data = min.Data;
                    min.Data = data;

                    wasDeleted = false;

                    newChild = Delete(node.Right, data, ref wasDeleted, ref wasSuccessful);
                    if (node.Right != newChild)
                    {
                        node.Right = newChild;
                    }

                    if (wasDeleted)
                    {
                        node.Balance--;
                    }
                }
                else if (node.Left == null)
                {
                    wasSuccessful = true;

#if TREE_WITH_PARENT_POINTERS
                    if (node.Right != null)
                    {
                        node.Right.Parent = node.Parent;
                    }
#endif
                    return node.Right;
                }
                else
                {
                    wasSuccessful = true;

#if TREE_WITH_PARENT_POINTERS
                    if (node.Left != null)
                    {
                        node.Left.Parent = node.Parent;
                    }
#endif
                    return node.Left;
                }
            }
            else
            {
                if (node.Right != null)
                {
                    newChild = Delete(node.Right, arg, ref wasDeleted, ref wasSuccessful);
                    if (node.Right != newChild)
                    {
                        node.Right = newChild;
                    }

                    if (wasDeleted)
                    {
                        node.Balance--;
                    }
                }
            }

            if (wasDeleted)
            {
                if (node.Balance == 1 || node.Balance == -1)
                {
                    wasDeleted = false;
                }
                else if (node.Balance == -2)
                {
                    var nodeLeft = node.Left;
                    int leftBalance = nodeLeft.Balance;

                    if (leftBalance == 1)
                    {
                        int leftRightBalance = nodeLeft.Right.Balance;

                        node.Left = RotateLeft(nodeLeft);
                        node = RotateRight(node);

                        node.Balance = 0;
                        node.Left.Balance = (leftRightBalance == 1) ? -1 : 0;
                        node.Right.Balance = (leftRightBalance == -1) ? 1 : 0;
                    }
                    else if (leftBalance == -1)
                    {
                        node = RotateRight(node);
                        node.Balance = 0;
                        node.Right.Balance = 0;
                    }
                    else if (leftBalance == 0)
                    {
                        node = RotateRight(node);
                        node.Balance = 1;
                        node.Right.Balance = -1;

                        wasDeleted = false;
                    }
                }
                else if (node.Balance == 2)
                {
                    var nodeRight = node.Right;
                    int rightBalance = nodeRight.Balance;

                    if (rightBalance == -1)
                    {
                        int rightLeftBalance = nodeRight.Left.Balance;

                        node.Right = RotateRight(nodeRight);
                        node = RotateLeft(node);

                        node.Balance = 0;
                        node.Left.Balance = (rightLeftBalance == 1) ? -1 : 0;
                        node.Right.Balance = (rightLeftBalance == -1) ? 1 : 0;
                    }
                    else if (rightBalance == 1)
                    {
                        node = RotateLeft(node);
                        node.Balance = 0;
                        node.Left.Balance = 0;
                    }
                    else if (rightBalance == 0)
                    {
                        node = RotateLeft(node);
                        node.Balance = -1;
                        node.Left.Balance = 1;

                        wasDeleted = false;
                    }
                }

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS
                node.Height = 1 + Math.Max(
                                        node.Left != null ? node.Left.Height : 0,
                                        node.Right != null ? node.Right.Height : 0);
#endif
            }

            return node;
        }

        /// <summary>
        /// Finds the min.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        private static Node<TValue> FindMin(Node<TValue> node)
        {
            while (node != null && node.Left != null)
            {
                node = node.Left;
            }

            return node;
        }

        /// <summary>
        /// Finds the max.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        private static Node<TValue> FindMax(Node<TValue> node)
        {
            while (node != null && node.Right != null)
            {
                node = node.Right;
            }

            return node;
        }

        /// <summary>
        /// Searches the specified subtree for the specified data.
        /// </summary>
        /// <param name="subtree">The subtree.</param>
        /// <param name="data">The data to search for.</param>
        /// <returns>null if not found, otherwise the node instance with the specified value</returns>
        private Node<TValue> Search(Node<TValue> subtree, TValue data)
        {
            if (subtree != null)
            {
                if (comparer.Compare(data, subtree.Data) < 0)
                {
                    return Search(subtree.Left, data);
                }
                if (comparer.Compare(data, subtree.Data) > 0)
                {
                    return Search(subtree.Right, data);
                }
                return subtree;
            }
            return null;
        }

        /// <summary>
        /// Deletes the min element in the tree.
        /// Precondition: (node != null)
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="wasDeleted"> </param>
        /// <param name="wasSuccessful"> </param>
        /// <returns></returns>
        private Node<TValue> DeleteMin(Node<TValue> node, ref bool wasDeleted, ref bool wasSuccessful)
        {
            Debug.Assert(node != null);

            if (node.Left == null)
            {
                wasDeleted = true;
                wasSuccessful = true;

#if TREE_WITH_PARENT_POINTERS
                if (node.Right != null)
                {
                    node.Right.Parent = node.Parent;
                }
#endif
                return node.Right;
            }

            node.Left = DeleteMin(node.Left, ref wasDeleted, ref wasSuccessful);
            if (wasDeleted)
            {
                node.Balance++;
            }

            if (wasDeleted)
            {
                if (node.Balance == 1 || node.Balance == -1)
                {
                    wasDeleted = false;
                }
                else if (node.Balance == -2)
                {
                    int leftBalance = node.Left.Balance;
                    if (leftBalance == 1)
                    {
                        int leftRightBalance = node.Left.Right.Balance;

                        node.Left = RotateLeft(node.Left);
                        node = RotateRight(node);

                        node.Balance = 0;
                        node.Left.Balance = (leftRightBalance == 1) ? -1 : 0;
                        node.Right.Balance = (leftRightBalance == -1) ? 1 : 0;
                    }
                    else if (leftBalance == -1)
                    {
                        node = RotateRight(node);
                        node.Balance = 0;
                        node.Right.Balance = 0;
                    }
                    else if (leftBalance == 0)
                    {
                        node = RotateRight(node);
                        node.Balance = 1;
                        node.Right.Balance = -1;

                        wasDeleted = false;
                    }
                }
                else if (node.Balance == 2)
                {
                    int rightBalance = node.Right.Balance;
                    if (rightBalance == -1)
                    {
                        int rightLeftBalance = node.Right.Left.Balance;

                        node.Right = RotateRight(node.Right);
                        node = RotateLeft(node);

                        node.Balance = 0;
                        node.Left.Balance = (rightLeftBalance == 1) ? -1 : 0;
                        node.Right.Balance = (rightLeftBalance == -1) ? 1 : 0;
                    }
                    else if (rightBalance == 1)
                    {
                        node = RotateLeft(node);
                        node.Balance = 0;
                        node.Left.Balance = 0;
                    }
                    else if (rightBalance == 0)
                    {
                        node = RotateLeft(node);
                        node.Balance = -1;
                        node.Left.Balance = 1;

                        wasDeleted = false;
                    }
                }

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS
                node.Height = 1 + Math.Max(
                                        node.Left != null ? node.Left.Height : 0,
                                        node.Right != null ? node.Right.Height : 0);
#endif
            }

            return node;
        }

        /// <summary>
        /// Deletes the max element in the tree.
        /// Precondition: (node != null)
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="wasDeleted"> </param>
        /// <param name="wasSuccessful"> </param>
        /// <returns></returns>
        private Node<TValue> DeleteMax(Node<TValue> node, ref bool wasDeleted, ref bool wasSuccessful)
        {
            Debug.Assert(node != null);

            if (node.Right == null)
            {
                wasDeleted = true;
                wasSuccessful = true;

#if TREE_WITH_PARENT_POINTERS
                if (node.Left != null)
                {
                    node.Left.Parent = node.Parent;
                }
#endif
                return node.Left;
            }

            node.Right = DeleteMax(node.Right, ref wasDeleted, ref wasSuccessful);
            if (wasDeleted)
            {
                node.Balance--;
            }

            if (wasDeleted)
            {
                if (node.Balance == 1 || node.Balance == -1)
                {
                    wasDeleted = false;
                }
                else if (node.Balance == -2)
                {
                    int leftBalance = node.Left.Balance;
                    if (leftBalance == 1)
                    {
                        int leftRightBalance = node.Left.Right.Balance;

                        node.Left = RotateLeft(node.Left);
                        node = RotateRight(node);

                        node.Balance = 0;
                        node.Left.Balance = (leftRightBalance == 1) ? -1 : 0;
                        node.Right.Balance = (leftRightBalance == -1) ? 1 : 0;
                    }
                    else if (leftBalance == -1)
                    {
                        node = RotateRight(node);
                        node.Balance = 0;
                        node.Right.Balance = 0;
                    }
                    else if (leftBalance == 0)
                    {
                        node = RotateRight(node);
                        node.Balance = 1;
                        node.Right.Balance = -1;

                        wasDeleted = false;
                    }
                }
                else if (node.Balance == 2)
                {
                    int rightBalance = node.Right.Balance;
                    if (rightBalance == -1)
                    {
                        int rightLeftBalance = node.Right.Left.Balance;

                        node.Right = RotateRight(node.Right);
                        node = RotateLeft(node);

                        node.Balance = 0;
                        node.Left.Balance = (rightLeftBalance == 1) ? -1 : 0;
                        node.Right.Balance = (rightLeftBalance == -1) ? 1 : 0;
                    }
                    else if (rightBalance == 1)
                    {
                        node = RotateLeft(node);
                        node.Balance = 0;
                        node.Left.Balance = 0;
                    }
                    else if (rightBalance == 0)
                    {
                        node = RotateLeft(node);
                        node.Balance = -1;
                        node.Left.Balance = 1;

                        wasDeleted = false;
                    }
                }

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS
                node.Height = 1 + Math.Max(
                                        node.Left != null ? node.Left.Height : 0,
                                        node.Right != null ? node.Right.Height : 0);
#endif
            }

            return node;
        }

        private void LeftRootRightVisit(Node<TValue> node, ref TValue[] dataArray, ref int position)
        {
            if (node == null)
            {
                return;
            }

            if (node.Left != null)
            {
                LeftRootRightVisit(node.Left, ref dataArray, ref position);
                //dataArray[position++] = node.Data;
            }

            dataArray[position++] = node.Data;

            if (node.Right != null)
            {
                LeftRootRightVisit(node.Right, ref dataArray, ref position);
                //dataArray[position++] = node.Data;  
            }
        }

        private void RightRootLeftVisit(Node<TValue> node, ref TValue[] dataArray, ref int position)
        {
            if (node == null)
            {
                return;
            }

            if (node.Right != null)
            {
                RightRootLeftVisit(node.Right, ref dataArray, ref position);
                //dataArray[position++] = node.Data;  
            }

            dataArray[position++] = node.Data;

            if (node.Left != null)
            {
                RightRootLeftVisit(node.Left, ref dataArray, ref position);
                //dataArray[position++] = node.Data;
            }
        }

        private void RootLeftRightVisit(Node<TValue> node, ref TValue[] dataArray, ref int position)
        {
            if (node == null)
            {
                return;
            }

            dataArray[position++] = node.Data;

            if (node.Left != null)
            {
                RootLeftRightVisit(node.Left, ref dataArray, ref position);
                //dataArray[position++] = node.Data;
            }

            if (node.Right != null)
            {
                RootLeftRightVisit(node.Right, ref dataArray, ref position);
                //dataArray[position++] = node.Data;  
            }
        }

#if TREE_WITH_PARENT_POINTERS

    /// <summary>
    /// Returns the predecessor of the specified node.
    /// </summary>
    /// <returns></returns>
        protected static Node<T> Predecesor(Node<T> node)
        {
            if (node.Left != null)
            {
                return FindMax(node.Left);
            }
            else
            {
                var p = node;
                while (p.Parent != null && p.Parent.Left == p)
                {
                    p = p.Parent;
                }

                return p.Parent;
            }
        }

        /// <summary>
        /// Returns the successor of the specified node.
        /// </summary>
        /// <returns></returns>
        protected static Node<T> Successor(Node<T> node)
        {
            if (node.Right != null)
            {
                return FindMin(node.Right);
            }
            else
            {
                var p = node;
                while (p.Parent != null && p.Parent.Right == p)
                {
                    p = p.Parent;
                }

                return p.Parent;
            }
        }

#endif

        /// <summary>
        /// Visits the tree using the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        private void Visit(VisitNodeHandler<Node<TValue>> visitor)
        {
            if (Root != null)
            {
                Root.Visit(visitor, 0);
            }
        }

        /// <summary>
        /// Rotates lefts this instance. 
        /// Precondition: (node != null && node.Right != null)
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private static Node<TValue> RotateLeft(Node<TValue> node)
        {
            Debug.Assert(node != null && node.Right != null);

            var right = node.Right;
            var nodeLeft = node.Left;
            var rightLeft = right.Left;

            node.Right = rightLeft;

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS
            node.Height = 1 + Math.Max(
                                    nodeLeft != null ? nodeLeft.Height : 0,
                                    rightLeft != null ? rightLeft.Height : 0);
#endif

#if TREE_WITH_PARENT_POINTERS
            var parent = node.Parent;
            if (rightLeft != null)
            {
                rightLeft.Parent = node;
            }
#endif
            right.Left = node;

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS
            right.Height = 1 + Math.Max(
                                    node.Height,
                                    right.Right != null ? right.Right.Height : 0);
#endif

#if TREE_WITH_PARENT_POINTERS
            node.Parent = right;
            if (parent != null)
            {
                if (parent.Left == node)
                {
                    parent.Left = right;
                }
                else
                {
                    parent.Right = right;
                }
            }

            right.Parent = parent;
#endif
            return right;
        }

        /// <summary>
        /// RotateRights this instance. 
        /// Precondition: (node != null && node.Left != null)
        /// </summary>
        /// <returns></returns>
        private static Node<TValue> RotateRight(Node<TValue> node)
        {
            Debug.Assert(node != null && node.Left != null);

            var left = node.Left;
            var leftRight = left.Right;
            node.Left = leftRight;

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS
            node.Height = 1 + Math.Max(
                                    leftRight != null ? leftRight.Height : 0,
                                    node.Right != null ? node.Right.Height : 0);
#endif

#if TREE_WITH_PARENT_POINTERS
            var parent = node.Parent;
            if (leftRight != null)
            {
                leftRight.Parent = node;
            }
#endif

            left.Right = node;

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS
            left.Height = 1 + Math.Max(
                                    left.Left != null ? left.Left.Height : 0,
                                    node.Height);
#endif

#if TREE_WITH_PARENT_POINTERS
            node.Parent = left;
            if (parent != null)
            {
                if (parent.Left == node)
                {
                    parent.Left = left;
                }
                else
                {
                    parent.Right = left;
                }
            }

            left.Parent = parent;
#endif
            return left;
        }

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS

    /// <summary>
    /// Concatenates the elements of the two trees. 
    /// Precondition: ALL elements of node2 must be LARGER than all elements of node1.
    /// </summary>
    /// <remarks>
    /// Complexity: 
    ///     Assuming height(node1) > height(node2), our procedure runs in height(node1) + height(node2) due to the two calls to findMin/deleteMin (or findMax, deleteMax respectively). 
    ///     Runs in O(height(node1)) if height(node1) == height(node2).
    /// Can be sped up.
    /// </remarks>
        protected Node<T> Concat(Node<T> node1, Node<T> node2)
        {
            if (node1 == null)
            {
                return node2;
            }
            else if (node2 == null)
            {
                return node1;
            }
            else
            {
                bool wasAdded = false, wasDeleted = false, wasSuccessful = false;

                int height1 = node1.Height;
                int height2 = node2.Height;

                if (height1 == height2)
                {
                    var result = new Node<T>() { Data = default(T), Left = node1, Right = node2, Balance = 0, Height = 1 + height1 };

#if TREE_WITH_PARENT_POINTERS
                    node1.Parent = result;
                    node2.Parent = result;
#endif
                    result = this.Delete(result, default(T), ref wasDeleted, ref wasSuccessful);
                    return result;
                }
                else if (height1 > height2)
                {
                    var min = FindMin(node2);
                    node2 = this.DeleteMin(node2, ref wasDeleted, ref wasSuccessful);

                    if (node2 != null)
                    {
                        node1 = this.ConcatImpl(node1, node2, min.Data, ref wasAdded);
                    }
                    else
                    {
                        node1 = this.Add(node1, min.Data, ref wasAdded, ref wasSuccessful);
                    }

                    return node1;
                }
                else
                {
                    var max = FindMax(node1);
                    node1 = this.DeleteMax(node1, ref wasDeleted, ref wasSuccessful);

                    if (node1 != null)
                    {
                        node2 = this.ConcatImpl(node2, node1, max.Data, ref wasAdded);
                    }
                    else
                    {
                        node2 = this.Add(node2, max.Data, ref wasAdded, ref wasSuccessful);
                    }

                    return node2;
                }
            }
        }

        /// <summary>
        /// Concatenates the specified trees. 
        /// Precondition: height(elem2add) is less than height(elem)
        /// </summary>
        /// <param name="elem">The elem</param>
        /// <param name="elemHeight">Height of the elem.</param>
        /// <param name="elem2add">The elem2add.</param>
        /// <param name="elem2AddHeight">Height of the elem2 add.</param>
        /// <param name="newData">The new data.</param>
        /// <param name="wasAdded">if set to <c>true</c> [was added].</param>
        /// <returns></returns>
        protected Node<T> ConcatImpl(Node<T> elem, Node<T> elem2add, T newData, ref bool wasAdded)
        {
            int heightDifference = elem.Height - elem2add.Height;

            if (elem == null)
            {
                if (heightDifference > 0)
                {
                    throw new ArgumentException("invalid input");
                }
            }
            else
            {
                int compareResult = this.comparer.Compare(elem.Data, newData);
                if (compareResult < 0)
                {
                    if (heightDifference == 0 || (heightDifference == 1 && elem.Balance == -1))
                    {
                        int balance = elem2add.Height - elem.Height;

                        elem = new Node<T>() { Data = newData, Left = elem, Right = elem2add, Balance = balance };
                        wasAdded = true;

#if TREE_WITH_PARENT_POINTERS
                        elem.Left.Parent = elem;
                        elem2add.Parent = elem;
#endif
                    }
                    else
                    {
                        elem.Right = this.ConcatImpl(elem.Right, elem2add, newData, ref wasAdded);

                        if (wasAdded)
                        {
                            elem.Balance++;
                            if (elem.Balance == 0)
                            {
                                wasAdded = false;
                            }
                        }

#if TREE_WITH_PARENT_POINTERS
                        elem.Right.Parent = elem;
#endif
                        if (elem.Balance == 2)
                        {
                            if (elem.Right.Balance == -1)
                            {
                                int elemRightLeftBalance = elem.Right.Left.Balance;

                                elem.Right = RotateRight(elem.Right);
                                elem = RotateLeft(elem);

                                elem.Balance = 0;
                                elem.Left.Balance = elemRightLeftBalance == 1 ? -1 : 0;
                                elem.Right.Balance = elemRightLeftBalance == -1 ? 1 : 0;

                                wasAdded = false;
                            }
                            else if (elem.Right.Balance == 1)
                            {
                                elem = RotateLeft(elem);

                                elem.Balance = 0;
                                elem.Left.Balance = 0;

                                wasAdded = false;
                            }
                            else if (elem.Right.Balance == 0)
                            {
                                ////special case for concat .. before adding the tree with smaller height to the tree with the bigger height, we find the correct insertion spot in the larger height tree.
                                ////because we balance the new subtree to be added which is normally done part of adding procedure, this situation isn't present in the adding procedure so we are catering for it here..

                                elem = RotateLeft(elem);

                                elem.Balance = -1;
                                elem.Left.Balance = 1;

                                wasAdded = true;
                            }
                        }
                    }
                }
                else if (compareResult > 0)
                {
                    if (heightDifference == 0 || (heightDifference == 1 && elem.Balance == 1))
                    {
                        int balance = elem.Height - elem2add.Height;

                        elem = new Node<T>() { Data = newData, Left = elem2add, Right = elem, Balance = balance };
                        wasAdded = true;

#if TREE_WITH_PARENT_POINTERS
                        elem.Right.Parent = elem;
                        elem2add.Parent = elem;
#endif
                    }
                    else
                    {
                        elem.Left = this.ConcatImpl(elem.Left, elem2add, newData, ref wasAdded);

                        if (wasAdded)
                        {
                            elem.Balance--;
                            if (elem.Balance == 0)
                            {
                                wasAdded = false;
                            }
                        }

#if TREE_WITH_PARENT_POINTERS
                        elem.Left.Parent = elem;
#endif
                        if (elem.Balance == -2)
                        {
                            if (elem.Left.Balance == 1)
                            {
                                int elemLeftRightBalance = elem.Left.Right.Balance;

                                elem.Left = RotateLeft(elem.Left);
                                elem = RotateRight(elem);

                                elem.Balance = 0;
                                elem.Left.Balance = elemLeftRightBalance == 1 ? -1 : 0;
                                elem.Right.Balance = elemLeftRightBalance == -1 ? 1 : 0;

                                wasAdded = false;
                            }
                            else if (elem.Left.Balance == -1)
                            {
                                elem = RotateRight(elem);
                                elem.Balance = 0;
                                elem.Right.Balance = 0;

                                wasAdded = false;
                            }
                            else if (elem.Left.Balance == 0)
                            {
                                ////special case for concat .. before adding the tree with smaller height to the tree with the bigger height, we find the correct insertion spot in the larger height tree.
                                ////because we balance the new subtree to be added which is normally done part of adding procedure, this situation isn't present in the adding procedure so we are catering for it here..

                                elem = RotateRight(elem);

                                elem.Balance = 1;
                                elem.Right.Balance = -1;

                                wasAdded = true;
                            }
                        }
                    }
                }

                elem.Height = 1 + Math.Max(
                                        elem.Left != null ? elem.Left.Height : 0,
                                        elem.Right != null ? elem.Right.Height : 0);
            }

            return elem;
        }

        /// <summary>
        /// This routine is used by the split procedure. Similar to concat except that the junction point is specified (i.e. the 'value' argument).
        /// ALL nodes in node1 tree have values less than the 'value' argument and ALL nodes in node2 tree have values greater than 'value'.
        /// Complexity: O(log N). 
        /// </summary>
        protected Node<T> ConcatAtJunctionPoint(Node<T> node1, Node<T> node2, T value)
        {
            bool wasAdded = false, wasSuccessful = false;

            if (node1 == null)
            {
                if (node2 != null)
                {
                    node2 = this.Add(node2, value, ref wasAdded, ref wasSuccessful);
                }
                else
                {
                    node2 = new Node<T> { Data = value, Balance = 0, Left = null, Right = null, Height = 1 };
                }

                return node2;
            }
            else if (node2 == null)
            {
                if (node1 != null)
                {
                    node1 = this.Add(node1, value, ref wasAdded, ref wasSuccessful);
                }
                else
                {
                    node1 = new Node<T> { Data = value, Balance = 0, Left = null, Right = null, Height = 1 };
                }

                return node1;
            }
            else
            {
                int height1 = node1.Height;
                int height2 = node2.Height;

                if (height1 == height2)
                {
                    // construct a new tree with its left and right subtrees pointing to the trees to be concatenated
                    var newNode = new Node<T>() { Data = value, Left = node1, Right = node2, Balance = 0, Height = 1 + height1 };

#if TREE_WITH_PARENT_POINTERS
                    node1.Parent = newNode;
                    node2.Parent = newNode;
#endif
                    return newNode;

                }
                else if (height1 > height2)
                {
                    // walk on node1's rightmost edge until you find the right place to insert the subtree with the smaller height (i.e. node2)
                    return this.ConcatImpl(node1, node2, value, ref wasAdded);
                }
                else
                {
                    // walk on node2's leftmost edge until you find the right place to insert the subtree with the smaller height (i.e. node1)
                    return this.ConcatImpl(node2, node1, value, ref wasAdded);
                }
            }
        }

        /// <summary>
        /// Splits this AVL tree instance into 2 AVL subtrees by the specified value.
        /// </summary>
        /// <param name="value">The value to use when splitting this instance.</param>
        /// <param name="mode">The mode specifying what to do with the value used for splitting. Options are not to include this value in either of the two resulting trees, include it in the left or include it in the right tree respectively</param>
        /// <param name="splitLeftTree">The split left avl tree. All values of this subtree are less than the value argument.</param>
        /// <param name="splitRightTree">The split right avl tree. All values of this subtree are greater than the value argument.</param>
        /// <returns></returns>
        protected Node<T> Split(
                    Node<T> elem,
                    T data,
                    ref Node<T> splitLeftTree,
                    ref Node<T> splitRightTree,
                    SplitOperationMode mode,
                    ref bool wasFound)
        {
            bool wasAdded = false, wasSuccessful = false;

            int compareResult = this.comparer.Compare(data, elem.Data);
            if (compareResult < 0)
            {
                this.Split(elem.Left, data, ref splitLeftTree, ref splitRightTree, mode, ref wasFound);
                if (wasFound)
                {
#if TREE_WITH_PARENT_POINTERS
                    if (elem.Right != null)
                    {
                        elem.Right.Parent = null;
                    }
#endif
                    splitRightTree = this.ConcatAtJunctionPoint(splitRightTree, elem.Right, elem.Data);
                }
            }
            else if (compareResult > 0)
            {
                this.Split(elem.Right, data, ref splitLeftTree, ref splitRightTree, mode, ref wasFound);
                if (wasFound)
                {
#if TREE_WITH_PARENT_POINTERS
                    if (elem.Left != null)
                    {
                        elem.Left.Parent = null;
                    }
#endif
                    splitLeftTree = this.ConcatAtJunctionPoint(elem.Left, splitLeftTree, elem.Data);
                }
            }
            else
            {
                wasFound = true;
                splitLeftTree = elem.Left;
                splitRightTree = elem.Right;

#if TREE_WITH_PARENT_POINTERS
                if (splitLeftTree != null)
                {
                    splitLeftTree.Parent = null;
                }

                if (splitRightTree != null)
                {
                    splitRightTree.Parent = null;
                }
#endif

                switch (mode)
                {
                    case SplitOperationMode.IncludeSplitValueToLeftSubtree:
                        splitLeftTree = this.Add(splitLeftTree, elem.Data, ref wasAdded, ref wasSuccessful);
                        break;
                    case SplitOperationMode.IncludeSplitValueToRightSubtree:
                        splitRightTree = this.Add(splitRightTree, elem.Data, ref wasAdded, ref wasSuccessful);
                        break;
                }
            }

            return elem;
        }

#endif

        #endregion

        #region Nested Classes

        /// <summary>
        /// node class
        /// </summary>
        /// <typeparam name="TElem">The type of the elem.</typeparam>
        internal class Node<TElem>
        {
            #region Properties

            public Node<TElem> Left { get; set; }

            public Node<TElem> Right { get; set; }

            public TElem Data { get; set; }

            public int Balance { get; set; }

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS
            public int Height { get; set; }
#endif

#if TREE_WITH_PARENT_POINTERS
            public Node<TElem> Parent { get; set; }
#endif

            #endregion

            #region Methods

            /// <summary>
            /// Visits (in-order) this node with the specified visitor.
            /// </summary>
            /// <param name="visitor">The visitor.</param>
            /// <param name="level">The level.</param>
            public void Visit(VisitNodeHandler<Node<TElem>> visitor, int level)
            {
                if (visitor == null)
                {
                    return;
                }

                if (Left != null)
                {
                    Left.Visit(visitor, level + 1);
                }

                visitor(this, level);

                if (Right != null)
                {
                    Right.Visit(visitor, level + 1);
                }
            }

            #endregion
        }

        #endregion
    }

    #endregion

}