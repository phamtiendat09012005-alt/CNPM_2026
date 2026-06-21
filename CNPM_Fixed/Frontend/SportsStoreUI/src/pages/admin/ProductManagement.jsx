import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import api, { getErrorMessage } from '../../services/api';

const money = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' });

export default function ProductManagement() {
  const [products, setProducts] = useState([]);
  const [error, setError] = useState('');

  const load = () => api.get('/admin/products').then((r) => setProducts(r.data.data)).catch((e) => setError(getErrorMessage(e)));
  useEffect(() => { load(); }, []);

  const toggle = async (product) => {
    try {
      await api.patch(`/admin/products/${product.id}/status`, !product.isActive);
      await load();
    } catch (e) { setError(getErrorMessage(e)); }
  };

  return (
    <div>
      <div className="page-title-row">
        <div><div className="eyebrow">QUẢN LÝ</div><h1>Sản phẩm</h1></div>
        <Link className="btn btn-primary" to="/admin/products/new">Thêm sản phẩm</Link>
      </div>
      {error && <div className="alert alert-error">{error}</div>}
      <div className="table-card">
        <table>
          <thead><tr><th>Sản phẩm</th><th>Danh mục</th><th>Giá</th><th>Tồn</th><th>Trạng thái</th><th></th></tr></thead>
          <tbody>
            {products.map((product) => (
              <tr key={product.id}>
                <td><div className="table-product"><img src={product.imageUrl || 'https://placehold.co/100x100?text=SP'} alt="" /><strong>{product.name}</strong></div></td>
                <td>{product.categoryName}</td>
                <td>{money.format(product.price)}</td>
                <td>{product.stock}</td>
                <td><span className={product.isActive ? 'status-badge status-completed' : 'status-badge status-cancelled'}>{product.isActive ? 'Đang bán' : 'Đã ẩn'}</span></td>
                <td className="action-cell">
                  <Link to={`/admin/products/${product.id}/edit`}>Sửa</Link>
                  <button className="text-button" onClick={() => toggle(product)}>{product.isActive ? 'Ẩn' : 'Hiện'}</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
