import { useEffect, useState } from 'react';
import api, { getErrorMessage } from '../../services/api';

export default function CategoryManagement() {
  const [categories, setCategories] = useState([]);
  const [form, setForm] = useState({ id: null, name: '', description: '' });
  const [error, setError] = useState('');

  const load = () => api.get('/admin/categories').then((r) => setCategories(r.data.data)).catch((e) => setError(getErrorMessage(e)));
  useEffect(() => { load(); }, []);

  const submit = async (event) => {
    event.preventDefault();
    try {
      if (form.id) await api.put(`/admin/categories/${form.id}`, { name: form.name, description: form.description });
      else await api.post('/admin/categories', { name: form.name, description: form.description });
      setForm({ id: null, name: '', description: '' });
      await load();
    } catch (e) { setError(getErrorMessage(e)); }
  };

  const toggle = async (item) => {
    try {
      await api.patch(`/admin/categories/${item.id}/status`, !item.isActive);
      await load();
    } catch (e) { setError(getErrorMessage(e)); }
  };

  return (
    <div>
      <div className="page-title-row"><h1>Danh mục</h1></div>
      {error && <div className="alert alert-error">{error}</div>}
      <div className="admin-two-column">
        <form className="form-card" onSubmit={submit}>
          <h3>{form.id ? 'Cập nhật danh mục' : 'Thêm danh mục'}</h3>
          <label>Tên danh mục</label>
          <input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} required />
          <label>Mô tả</label>
          <textarea value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
          <button className="btn btn-primary btn-block">{form.id ? 'Cập nhật' : 'Thêm danh mục'}</button>
          {form.id && <button type="button" className="btn btn-ghost btn-block" onClick={() => setForm({ id: null, name: '', description: '' })}>Hủy sửa</button>}
        </form>
        <div className="table-card">
          <table>
            <thead><tr><th>Tên</th><th>Mô tả</th><th>Trạng thái</th><th></th></tr></thead>
            <tbody>
              {categories.map((item) => (
                <tr key={item.id}>
                  <td><strong>{item.name}</strong></td>
                  <td>{item.description}</td>
                  <td>{item.isActive ? 'Hoạt động' : 'Đã ẩn'}</td>
                  <td className="action-cell">
                    <button className="text-button" onClick={() => setForm({ id: item.id, name: item.name, description: item.description || '' })}>Sửa</button>
                    <button className="text-button" onClick={() => toggle(item)}>{item.isActive ? 'Ẩn' : 'Hiện'}</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
