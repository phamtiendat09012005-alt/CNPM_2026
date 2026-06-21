import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { cartService } from '../../services/cartService';
import { getErrorMessage } from '../../services/api';

const money = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' });

export default function Cart() {
  const [cart, setCart] = useState(null);
  const [error, setError] = useState('');

  const load = () => cartService.getCart().then(setCart).catch((e) => setError(getErrorMessage(e)));
  useEffect(() => { load(); }, []);

  const update = async (item, quantity = item.quantity, isSelected = item.isSelected) => {
    try {
      setCart(await cartService.updateItem(item.id, quantity, isSelected));
    } catch (requestError) {
      setError(getErrorMessage(requestError));
    }
  };

  const remove = async (id) => {
    try { setCart(await cartService.removeItem(id)); } catch (e) { setError(getErrorMessage(e)); }
  };

  if (!cart) return <div className="container section"><div className="state-card">Đang tải giỏ hàng...</div></div>;

  return (
    <div className="container section">
      <div className="page-title-row"><h1>Giỏ hàng</h1><span>{cart.items.length} sản phẩm</span></div>
      {error && <div className="alert alert-error">{error}</div>}
      {cart.items.length === 0 ? (
        <div className="state-card"><p>Giỏ hàng đang trống.</p><Link className="btn btn-primary" to="/products">Tiếp tục mua sắm</Link></div>
      ) : (
        <div className="cart-layout">
          <div className="cart-list">
            {cart.items.map((item) => (
              <article className="cart-item" key={item.id}>
                <input type="checkbox" checked={item.isSelected} onChange={(e) => update(item, item.quantity, e.target.checked)} />
                <img src={item.imageUrl || 'https://placehold.co/180x180?text=Product'} alt={item.productName} />
                <div className="cart-item-info">
                  <Link to={`/product/${item.productId}`}>{item.productName}</Link>
                  <span>{item.size || 'Mặc định'} · {item.color || 'Mặc định'}</span>
                  <strong>{money.format(item.unitPrice)}</strong>
                </div>
                <input
                  className="quantity-input"
                  type="number"
                  min="1"
                  max={item.stockQuantity}
                  value={item.quantity}
                  onChange={(e) => update(item, Math.max(1, Number(e.target.value)), item.isSelected)}
                />
                <strong>{money.format(item.lineTotal)}</strong>
                <button className="text-button danger" onClick={() => remove(item.id)}>Xóa</button>
              </article>
            ))}
          </div>
          <aside className="summary-card">
            <h3>Tóm tắt</h3>
            <div><span>Sản phẩm đã chọn</span><strong>{cart.selectedQuantity}</strong></div>
            <div className="summary-total"><span>Tạm tính</span><strong>{money.format(cart.selectedTotal)}</strong></div>
            <Link className="btn btn-primary btn-block" to="/checkout">Tiến hành đặt hàng</Link>
          </aside>
        </div>
      )}
    </div>
  );
}
