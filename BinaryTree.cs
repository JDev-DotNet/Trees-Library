using System;
using System.Collections.Generic;
using System.Globalization;

namespace JDev.Trees
{
    #region class BinaryTreeOutputData<TKey, TValue>

    /// <summary>
    /// Class nhận dữ liệu từ cây
    /// </summary>
    /// <typeparam name="TKey">Kiểu dữ liệu của khóa của dữ liệu</typeparam>
    /// <typeparam name="TValue">Kiểu dữ liệu của dữ liệu</typeparam>
    [Serializable()]
    public sealed class BinaryTreeOutputData<TKey, TValue>
    {
        /// <summary>
        /// Dữ liệu
        /// </summary>
        public TValue Data;

        /// <summary>
        /// Khóa của dữ liệu
        /// </summary>
        public TKey Key;
    }

    #endregion

    #region class BinaryTree<TValue>

    /// <summary>
    /// Cây nhị phân
    /// </summary>
    /// <typeparam name="TValue">Kiểu dữ liệu của dữ liệu</typeparam>
    [Serializable()]
    public sealed class BinaryTree<TValue>
    {
        #region class BinaryTreeNode<T>

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        internal class BinaryTreeNode<T>
        {
            public T Data;
            public BinaryTreeNode<T> Left;
            public BinaryTreeNode<T> Right;
        }

        #endregion

        #region Ctor

        /// <summary>
        /// Khởi tạo cây nhị phân. Dữ liệu đã kế thừa Interface IComparable.
        /// </summary>
        public BinaryTree()
        {
            comparer = GetComparer();
        }

        /// <summary>
        /// Khởi tạo cây nhị phân
        /// </summary>
        /// <param name="comparer">Interface so sánh</param>
        public BinaryTree(IComparer<TValue> comparer)
        {
            this.comparer = comparer;
        }

        #endregion

        #region Private Methods

        private IComparer<TValue> GetComparer()
        {
            if (typeof(IComparable<TValue>).IsAssignableFrom(typeof(TValue)) ||
                typeof(IComparable<TValue>).IsAssignableFrom(typeof(TValue)))
            {
                return Comparer<TValue>.Default;
            }
            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                                                              "The type {0} cannot be compared. It must implement IComparable<T> or IComparable interface",
                                                              typeof(TValue).FullName));
        }

        private bool Add(ref BinaryTreeNode<TValue> binaryTreeNode, TValue data)
        {
            if (binaryTreeNode == null)
            {
                binaryTreeNode = new BinaryTreeNode<TValue> { Data = data, Left = null, Right = null };
                return true;
            }

            if (comparer.Compare(binaryTreeNode.Data, data) < 0)
            {
                return Add(ref binaryTreeNode.Right, data);
            }

            if (comparer.Compare(binaryTreeNode.Data, data) > 0)
            {
                return Add(ref binaryTreeNode.Left, data);
            }

            return false;
        }

        private BinaryTreeNode<TValue> FindVictim(ref BinaryTreeNode<TValue> binaryTreeNode)
        {
            if (binaryTreeNode == null)
            {
                return null;
            }

            if (binaryTreeNode.Left == null)
            {
                return binaryTreeNode;
            }

            if (binaryTreeNode.Left != null)
            {
                return FindVictim(ref binaryTreeNode.Left);
            }

            return null;
        }

        private void Swap(ref TValue x, ref TValue y)
        {
            TValue temp = y;
            y = x;
            x = temp;
        }

        private bool Delete(ref BinaryTreeNode<TValue> binaryTreeNode, ref TValue data)
        {
            if (binaryTreeNode == null)
            {
                return false;
            }

            if (comparer.Compare(binaryTreeNode.Data, data) == 0)
            {
                BinaryTreeNode<TValue> temp = FindVictim(ref binaryTreeNode.Right);

                if (temp == null)
                {
                    if (binaryTreeNode.Left == null)
                    {
                        binaryTreeNode = null;
                        return true;
                    }
                    BinaryTreeNode<TValue> temp3 = binaryTreeNode.Left;
                    binaryTreeNode.Data = temp3.Data;
                    binaryTreeNode.Right = temp3.Right;
                    binaryTreeNode.Left = temp3.Left;
                    return true;
                }

                if (temp.Right == null)
                {
                    Swap(ref binaryTreeNode.Data, ref temp.Data);
                    temp = null;
                    return true;
                }

                Swap(ref binaryTreeNode.Data, ref temp.Data);

                BinaryTreeNode<TValue> temp2 = temp.Right;
                temp.Data = temp2.Data;
                temp.Right = temp2.Right;
                temp.Left = temp2.Left;

                return true;
            }

            if (comparer.Compare(binaryTreeNode.Data, data) < 0)
            {
                return Delete(ref binaryTreeNode.Right, ref data);
            }

            if (comparer.Compare(binaryTreeNode.Data, data) > 0)
            {
                return Delete(ref binaryTreeNode.Left, ref data);
            }

            return false;
        }

        private TValue Search(BinaryTreeNode<TValue> binaryTreeNode, TValue data)
        {
            if (binaryTreeNode == null)
            {
                return default(TValue);
            }

            if (comparer.Compare(binaryTreeNode.Data, data) < 0)
            {
                return Search(binaryTreeNode.Right, data);
            }

            if (comparer.Compare(binaryTreeNode.Data, data) > 0)
            {
                return Search(binaryTreeNode.Left, data);
            }

            if (comparer.Compare(binaryTreeNode.Data, data) == 0)
            {
                return binaryTreeNode.Data;
            }

            return default(TValue);
        }

        private bool Exists(BinaryTreeNode<TValue> binaryTreeNode, TValue data)
        {
            if (binaryTreeNode == null)
            {
                return false;
            }

            if (comparer.Compare(binaryTreeNode.Data, data) < 0)
            {
                return Exists(binaryTreeNode.Right, data);
            }

            if (comparer.Compare(binaryTreeNode.Data, data) > 0)
            {
                return Exists(binaryTreeNode.Left, data);
            }

            if (comparer.Compare(binaryTreeNode.Data, data) == 0)
            {
                return true;
            }

            return false;
        }

        private void Count(BinaryTreeNode<TValue> binaryTreeNode, ref int count)
        {
            if (binaryTreeNode == null)
            {
                return;
            }

            count++;

            Count(binaryTreeNode.Left, ref count);
            Count(binaryTreeNode.Right, ref count);
        }

        private void LeftRootRightVisit(ref BinaryTreeNode<TValue> node, ref TValue[] dataArray, ref int position)
        {
            if (node == null)
            {
                return;
            }

            if (node.Left != null)
            {
                LeftRootRightVisit(ref node.Left, ref dataArray, ref position);
                //dataArray[position++] = node.Data;
            }

            dataArray[position++] = node.Data;

            if (node.Right != null)
            {
                LeftRootRightVisit(ref node.Right, ref dataArray, ref position);
                //dataArray[position++] = node.Data;  
            }
        }

        private void RightRootLeftVisit(ref BinaryTreeNode<TValue> node, ref TValue[] dataArray, ref int position)
        {
            if (node == null)
            {
                return;
            }

            if (node.Right != null)
            {
                RightRootLeftVisit(ref node.Right, ref dataArray, ref position);
                //dataArray[position++] = node.Data;  
            }

            dataArray[position++] = node.Data;

            if (node.Left != null)
            {
                RightRootLeftVisit(ref node.Left, ref dataArray, ref position);
                //dataArray[position++] = node.Data;
            }
        }

        private void RootLeftRightVisit(ref BinaryTreeNode<TValue> node, ref TValue[] dataArray, ref int position)
        {
            if (node == null)
            {
                return;
            }

            dataArray[position++] = node.Data;

            if (node.Left != null)
            {
                RootLeftRightVisit(ref node.Left, ref dataArray, ref position);
                //dataArray[position++] = node.Data;
            }

            if (node.Right != null)
            {
                RootLeftRightVisit(ref node.Right, ref dataArray, ref position);
                //dataArray[position++] = node.Data;  
            }
        }

        #endregion

        #region Public Methods

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
                LeftRootRightVisit(ref root, ref dataArray, ref position);
            }

            if (visitMode == VisitMode.RightRootLeft)
            {
                RightRootLeftVisit(ref root, ref dataArray, ref position);
            }

            if (visitMode == VisitMode.RootLeftRight)
            {
                RootLeftRightVisit(ref root, ref dataArray, ref position);
            }

            return dataArray;
        }

        /// <summary>
        /// Thêm dữ liệu vào cây
        /// </summary>
        /// <param name="data">Dữ liệu thêm vào cây</param>
        /// <returns>Trả về true nếu thêm thành công, trả về false nếu dữ liệu đã có trong cây</returns>
        public bool Add(TValue data)
        {
            return Add(ref root, data);
        }

        /// <summary>
        /// Kiểm tra cây có dữ liệu hay không
        /// </summary>
        /// <returns>Trả về true nếu cây không có dữ liệu và ngược lại</returns>
        public bool IsEmpty()
        {
            return root == null;
        }

        /// <summary>
        /// Xóa tất cả dữ liệu trong cây
        /// </summary>
        public void Clear()
        {
            root = null;
        }

        /// <summary>
        /// Xóa dữ liệu trên cây
        /// </summary>
        /// <param name="data">Dữ liệu</param>
        /// <returns>Trả về true nếu xóa thành công, trả về false khi dữ liệu không tồn tại trên cây</returns>
        public bool Delete(TValue data)
        {
            if (root == null)
            {
                return false;
            }

            return Delete(ref root, ref data);
        }

        /// <summary>
        /// Tìm kiếm dữ liệu trong cây
        /// </summary>
        /// <param name="data">Dữ liệu cần tìm</param>
        /// <returns>Dữ liệu lấy ra</returns>
        /// <remarks>Sự thay đổi dữ liệu có thể phá vỡ tính đúng đắn của cây</remarks>
        public TValue Search(TValue data)
        {
            return Search(root, data);
        }

        /// <summary>
        /// Kiểm tra dữ liệu đã có trong cây hay chưa
        /// </summary>
        /// <param name="data">Dữ liệu</param>
        /// <returns>Trả về true nếu dữ liệu đã có trong cây, và ngược lại</returns>
        public bool Exists(TValue data)
        {
            return Exists(root, data);
        }

        /// <summary>
        /// Lấy số lượng phần tử trong cây
        /// </summary>
        /// <returns>Số lượng phần tử trong cây</returns>
        public int Count()
        {
            int count = 0;
            Count(root, ref count);
            return count;
        }

        /// <summary>
        /// Tạo bản sao của cây
        /// </summary>
        /// <returns>Bản sao của cây</returns>
        public BinaryTree<TValue> Clone()
        {
            return (BinaryTree<TValue>)MemberwiseClone();
        }

        #endregion

        #region Fields

        private BinaryTreeNode<TValue> root;

        private IComparer<TValue> comparer;

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

        #endregion
    }

    #endregion

    #region class BinaryTree<TKey, TValue>

    /// <summary>
    /// Cây nhị phân
    /// </summary>
    /// <typeparam name="TValue">Kiểu dữ liệu của dữ liệu</typeparam>
    /// <typeparam name="TKey">Kiểu dữ liệu của khóa của dữ liệu</typeparam>
    [Serializable()]
    public sealed class BinaryTree<TKey, TValue>
    {
        #region class BinaryTreeNode<TypeOfKey, TypeOfData>

        internal class BinaryTreeNode<TypeOfKey, TypeOfData>
        {
            public TypeOfData Data;
            public TypeOfKey Key;
            public BinaryTreeNode<TypeOfKey, TypeOfData> Left;
            public BinaryTreeNode<TypeOfKey, TypeOfData> Right;
        }

        #endregion

        #region Ctor

        /// <summary>
        /// Khởi tạo cây nhị phân. Dữ liệu đã kế thừa Interface IComparable.
        /// </summary>
        public BinaryTree()
        {
            comparer = GetComparer();
        }

        /// <summary>
        /// Khởi tạo cây nhị phân
        /// </summary>
        /// <param name="comparer">Interface so sánh</param>
        public BinaryTree(IComparer<TKey> comparer)
        {
            this.comparer = comparer;
        }

        #endregion

        #region Private Methods

        private IComparer<TKey> GetComparer()
        {
            if (typeof(IComparable<TKey>).IsAssignableFrom(typeof(TKey)) ||
                typeof(IComparable).IsAssignableFrom(typeof(TKey)))
            {
                return Comparer<TKey>.Default;
            }
            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                                                              "The type {0} cannot be compared. It must implement IComparable<T> or IComparable interface",
                                                              typeof(TKey).FullName));
        }

        private bool Add(ref BinaryTreeNode<TKey, TValue> binaryTreeNode, ref TKey key, ref TValue data)
        {
            if (binaryTreeNode == null)
            {
                binaryTreeNode = new BinaryTreeNode<TKey, TValue> { Key = key, Data = data, Left = null, Right = null };
                return true;
            }

            if (comparer.Compare(binaryTreeNode.Key, key) < 0)
            {
                return Add(ref binaryTreeNode.Right, ref key, ref data);
            }

            if (comparer.Compare(binaryTreeNode.Key, key) > 0)
            {
                return Add(ref binaryTreeNode.Left, ref key, ref data);
            }

            return false;
        }

        private BinaryTreeNode<TKey, TValue> FindVictim(ref BinaryTreeNode<TKey, TValue> binaryTreeNode)
        {
            if (binaryTreeNode == null)
            {
                return null;
            }

            if (binaryTreeNode.Left == null)
            {
                return binaryTreeNode;
            }

            if (binaryTreeNode.Left != null)
            {
                return FindVictim(ref binaryTreeNode.Left);
            }

            return null;
        }

        private void Swap(ref TValue x, ref TValue y)
        {
            TValue temp = y;
            y = x;
            x = temp;
        }

        private void Swap(ref TKey x, ref TKey y)
        {
            TKey temp = y;
            y = x;
            x = temp;
        }

        private bool Delete(ref BinaryTreeNode<TKey, TValue> binaryTreeNode, ref TKey key)
        {
            if (binaryTreeNode == null)
            {
                return false;
            }

            if (comparer.Compare(binaryTreeNode.Key, key) == 0)
            {
                BinaryTreeNode<TKey, TValue> temp = FindVictim(ref binaryTreeNode.Right);

                if (temp == null)
                {
                    if (binaryTreeNode.Left == null)
                    {
                        binaryTreeNode = null;
                        return true;
                    }
                    BinaryTreeNode<TKey, TValue> temp3 = binaryTreeNode.Left;
                    binaryTreeNode.Data = temp3.Data;
                    binaryTreeNode.Key = temp3.Key;
                    binaryTreeNode.Right = temp3.Right;
                    binaryTreeNode.Left = temp3.Left;
                    return true;
                }

                if (temp.Right == null)
                {
                    Swap(ref binaryTreeNode.Data, ref temp.Data);
                    temp = null;
                    return true;
                }

                Swap(ref binaryTreeNode.Data, ref temp.Data);
                Swap(ref binaryTreeNode.Key, ref temp.Key);

                BinaryTreeNode<TKey, TValue> temp2 = temp.Right;
                temp.Data = temp2.Data;
                temp.Key = temp2.Key;
                temp.Right = temp2.Right;
                temp.Left = temp2.Left;

                return true;
            }

            if (comparer.Compare(binaryTreeNode.Key, key) < 0)
            {
                return Delete(ref binaryTreeNode.Right, ref key);
            }

            if (comparer.Compare(binaryTreeNode.Key, key) > 0)
            {
                return Delete(ref binaryTreeNode.Left, ref key);
            }

            return false;
        }

        private TValue Search(BinaryTreeNode<TKey, TValue> binaryTreeNode, TKey key)
        {
            if (binaryTreeNode == null)
            {
                return default(TValue);
            }

            if (comparer.Compare(binaryTreeNode.Key, key) < 0)
            {
                return Search(binaryTreeNode.Right, key);
            }

            if (comparer.Compare(binaryTreeNode.Key, key) > 0)
            {
                return Search(binaryTreeNode.Left, key);
            }

            if (comparer.Compare(binaryTreeNode.Key, key) == 0)
            {
                return binaryTreeNode.Data;
            }

            return default(TValue);
        }

        private bool Exists(BinaryTreeNode<TKey, TValue> binaryTreeNode, ref TKey key)
        {
            if (binaryTreeNode == null)
            {
                return false;
            }

            if (comparer.Compare(binaryTreeNode.Key, key) < 0)
            {
                return Exists(binaryTreeNode.Right, ref key);
            }

            if (comparer.Compare(binaryTreeNode.Key, key) > 0)
            {
                return Exists(binaryTreeNode.Left, ref key);
            }

            if (comparer.Compare(binaryTreeNode.Key, key) == 0)
            {
                return true;
            }

            return false;
        }

        private void Count(BinaryTreeNode<TKey, TValue> binaryTreeNode, ref int count)
        {
            if (binaryTreeNode == null)
            {
                return;
            }

            count++;

            Count(binaryTreeNode.Left, ref count);
            Count(binaryTreeNode.Right, ref count);
        }

        private void LeftRootRightVisit(ref BinaryTreeNode<TKey, TValue> node, ref BinaryTreeOutputData<TKey, TValue>[] dataArray, ref int position)
        {
            if (node == null)
            {
                return;
            }

            if (node.Left != null)
            {
                LeftRootRightVisit(ref node.Left, ref dataArray, ref position);
                //dataArray[position++] = node.Data;
            }

            dataArray[position] = new BinaryTreeOutputData<TKey, TValue> { Data = node.Data, Key = node.Key };
            position++;

            if (node.Right != null)
            {
                LeftRootRightVisit(ref node.Right, ref dataArray, ref position);
                //dataArray[position++] = node.Data;  
            }
        }

        private void RightRootLeftVisit(ref BinaryTreeNode<TKey, TValue> node, ref BinaryTreeOutputData<TKey, TValue>[] dataArray, ref int position)
        {
            if (node == null)
            {
                return;
            }

            if (node.Right != null)
            {
                RightRootLeftVisit(ref node.Right, ref dataArray, ref position);
                //dataArray[position++] = node.Data;  
            }

            dataArray[position] = new BinaryTreeOutputData<TKey, TValue> { Data = node.Data, Key = node.Key };
            position++;

            if (node.Left != null)
            {
                RightRootLeftVisit(ref node.Left, ref dataArray, ref position);
                //dataArray[position++] = node.Data;
            }
        }

        private void RootLeftRightVisit(ref BinaryTreeNode<TKey, TValue> node, ref BinaryTreeOutputData<TKey, TValue>[] dataArray, ref int position)
        {
            if (node == null)
            {
                return;
            }

            dataArray[position] = new BinaryTreeOutputData<TKey, TValue> { Data = node.Data, Key = node.Key };
            position++;

            if (node.Left != null)
            {
                RootLeftRightVisit(ref node.Left, ref dataArray, ref position);
                //dataArray[position++] = node.Data;
            }

            if (node.Right != null)
            {
                RootLeftRightVisit(ref node.Right, ref dataArray, ref position);
                //dataArray[position++] = node.Data;  
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Thêm dữ liệu vào cây
        /// </summary>
        /// <param name="key">Khóa của dữ liệu </param>
        /// <param name="data">Dữ liệu thêm vào cây</param>
        /// <returns>Trả về true nếu thêm thành công, trả về false nếu dữ liệu đã có trong cây</returns>
        public bool Add(TKey key, TValue data)
        {
            return Add(ref root, ref key, ref data);
        }

        /// <summary>
        /// Kiểm tra cây có dữ liệu hay không
        /// </summary>
        /// <returns>Trả về true nếu cây không có dữ liệu và ngược lại</returns>
        public bool IsEmpty()
        {
            return root == null;
        }

        /// <summary>
        /// Xóa tất cả dữ liệu trong cây
        /// </summary>
        public void Clear()
        {
            root = null;
        }

        /// <summary>
        /// Xóa dữ liệu trên cây
        /// </summary>
        /// <param name="key">Khóa của dữ liệu </param>
        /// <returns>Trả về true nếu xóa thành công, trả về false khi dữ liệu không tồn tại trên cây</returns>
        public bool Delete(TKey key)
        {
            if (root == null)
            {
                return false;
            }

            return Delete(ref root, ref key);
        }

        /// <summary>
        /// Tìm kiếm dữ liệu trong cây theo khóa của dữ liệu
        /// </summary>
        /// <param name="key">Khóa của dữ liệu</param>
        /// <returns>Dữ liệu lấy ra</returns>
        public TValue Search(TKey key)
        {
            return Search(root, key);
        }

        /// <summary>
        /// Tìm kiếm dữ liệu trong cây theo khóa của dữ liệu
        /// </summary>
        /// <param name="key">Khóa của dữ liệu</param>
        /// <param name="data">Dữ liệu lấy ra</param>
        /// <returns>Trả về true nếu tìm thấy và ngược lại</returns>
        public bool Search(TKey key, out TValue data)
        {
            data = Search(root, key);

            if (data.Equals(null))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Kiểm tra dữ liệu đã có trong cây hay chưa
        /// </summary>
        /// <param name="key">Khóa của dữ liệu</param>
        /// <returns>Trả về true nếu dữ liệu đã có trong cây, và ngược lại</returns>
        public bool Exists(TKey key)
        {
            return Exists(root, ref key);
        }

        /// <summary>
        /// Lấy số lượng phần tử trong cây
        /// </summary>
        /// <returns>Số lượng phần tử trong cây</returns>
        public int Count()
        {
            int count = 0;
            Count(root, ref count);
            return count;
        }

        /// <summary>
        /// Trả về mảng dữ liệu trong cây 
        /// </summary>
        /// <param name="visitMode">Kiểu duyệt cây</param>
        /// <returns>Mảng dữ liệu</returns>
        public BinaryTreeOutputData<TKey, TValue>[] ToArray(VisitMode visitMode = VisitMode.LeftRootRight)
        {
            var dataArray = new BinaryTreeOutputData<TKey, TValue>[Count()];

            var position = 0;

            if (visitMode == VisitMode.LeftRootRight)
            {
                LeftRootRightVisit(ref root, ref dataArray, ref position);
            }

            if (visitMode == VisitMode.RightRootLeft)
            {
                RightRootLeftVisit(ref root, ref dataArray, ref position);
            }

            if (visitMode == VisitMode.RootLeftRight)
            {
                RootLeftRightVisit(ref root, ref dataArray, ref position);
            }

            return dataArray;
        }

        /// <summary>
        /// Tạo bản sao của cây
        /// </summary>
        /// <returns></returns>
        public BinaryTree<TKey, TValue> Clone()
        {
            return (BinaryTree<TKey, TValue>)MemberwiseClone();
        }

        #endregion

        #region Fields

        private BinaryTreeNode<TKey, TValue> root;

        private IComparer<TKey> comparer;

        #endregion

        #region Properties

        /// <summary>
        /// Lấy, gán Interface so sánh
        /// </summary>
        public IComparer<TKey> Comparer
        {
            set { comparer = value; }
            get { return comparer; }
        }

        #endregion
    }

    #endregion

}