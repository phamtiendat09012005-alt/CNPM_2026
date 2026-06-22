import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { cartService } from '../../services/cartService';
import { orderService } from '../../services/orderService';
import { getErrorMessage } from '../../services/api';

const money = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' });

export default function Checkout() {
  const navigate = useNavigate();
  const [cart, setCart] = useState(null);
  const [form, setForm] = useState({ receiverName: '', receiverPhone: '', shippingAddress: '', paymentMethod: 'COD', note: '' });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  useEffect(() => { cartService.getCart().then(setCart).catch((e) => setError(getErrorMessage(e))); }, []);
  const update = (event) => setForm({ ...form, [event.target.name]: event.target.value });

  const submit = async (event) => {
    event.preventDefault();
    setLoading(true);
    setError('');
    try {
      const order = await orderService.checkout(form);
      if (order.requiresDemoPayment) {
        await orderService.simulatePayment(order.id);
      }
      navigate('/order-success', { state: { order } });
    } catch (requestError) {
      setError(getErrorMessage(requestError));
    } finally {
      setLoading(false);
    }
  };

  if (!cart) return <div className="container section"><div className="state-card">Đang tải...</div></div>;

  return (
    <div className="container section">
      <div className="page-title-row"><h1>Thanh toán</h1></div>
      <form className="checkout-layout" onSubmit={submit}>
        <section className="form-card">
          <h3>Thông tin nhận hàng</h3>
          {error && <div className="alert alert-error">{error}</div>}
          <label>Người nhận</label>
          <input name="receiverName" value={form.receiverName} onChange={update} required />
          <label>Số điện thoại</label>
          <input name="receiverPhone" value={form.receiverPhone} onChange={update} required />
          <label>Địa chỉ giao hàng</label>
          <textarea name="shippingAddress" value={form.shippingAddress} onChange={update} required />
          <label>Ghi chú</label>
          <textarea name="note" value={form.note} onChange={update} />
          <h3>Phương thức thanh toán</h3>
          <label className="radio-row"><input type="radio" name="paymentMethod" value="COD" checked={form.paymentMethod === 'COD'} onChange={update} /> Thanh toán khi nhận hàng</label>
          <label className="radio-row"><input type="radio" name="paymentMethod" value="OnlineGateway" checked={form.paymentMethod === 'OnlineGateway'} onChange={update} /> Thanh toán online mô phỏng</label>
        </section>
        <aside className="summary-card">
          <h3>Đơn hàng</h3>
          {cart.items.filter((x) => x.isSelected).map((item) => (
            <div key={item.id}><span>{item.productName} × {item.quantity}</span><strong>{money.format(item.lineTotal)}</strong></div>
          ))}
          <div><span>Tạm tính</span><strong>{money.format(cart.selectedTotal)}</strong></div>
          <div><span>Phí giao hàng</span><strong>{cart.selectedTotal >= 1000000 ? 'Miễn phí' : money.format(30000)}</strong></div>
          <div className="summary-total"><span>Tổng cộng</span><strong>{money.format(cart.selectedTotal + (cart.selectedTotal >= 1000000 ? 0 : 30000))}</strong></div>
          <button className="btn btn-primary btn-block" disabled={loading}>{loading ? 'Đang tạo đơn...' : 'Xác nhận đặt hàng'}</button>
        </aside>
      </form>
    </div>
  );
}
