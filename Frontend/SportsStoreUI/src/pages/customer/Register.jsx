import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { authService } from '../../services/authService';
import { useAuth } from '../../context/AuthContext';
import { getErrorMessage } from '../../services/api';

export default function Register() {
  const [form, setForm] = useState({ fullName: '', email: '', phoneNumber: '', password: '' });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();

  const submit = async (event) => {
    event.preventDefault();
    setError('');
    setLoading(true);
    try {
      const data = await authService.register(form);
      login(data);
      navigate('/');
    } catch (requestError) {
      setError(getErrorMessage(requestError));
    } finally {
      setLoading(false);
    }
  };

  const update = (event) => setForm({ ...form, [event.target.name]: event.target.value });

  return (
    <div className="auth-page">
      <form className="auth-card" onSubmit={submit}>
        <Link className="brand centered" to="/">SPORTS<span>AI</span></Link>
        <h1>Tạo tài khoản</h1>
        <p>Đăng ký để mua hàng và nhận gợi ý AI.</p>
        {error && <div className="alert alert-error">{error}</div>}
        <label>Họ và tên</label>
        <input name="fullName" value={form.fullName} onChange={update} required />
        <label>Email</label>
        <input name="email" type="email" value={form.email} onChange={update} required />
        <label>Số điện thoại</label>
        <input name="phoneNumber" value={form.phoneNumber} onChange={update} />
        <label>Mật khẩu</label>
        <input name="password" type="password" value={form.password} onChange={update} required />
        <small>Mật khẩu từ 8 ký tự, có chữ hoa, chữ thường, số và ký tự đặc biệt.</small>
        <button className="btn btn-primary btn-block" disabled={loading}>
          {loading ? 'Đang đăng ký...' : 'Đăng ký'}
        </button>
        <div className="auth-footer">Đã có tài khoản? <Link to="/login">Đăng nhập</Link></div>
      </form>
    </div>
  );
}
