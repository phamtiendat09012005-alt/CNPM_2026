import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import api, { getErrorMessage } from '../../services/api';
import { productService } from '../../services/productService';

const emptyVariant = { sku: '', size: '', color: '', price: 0, stockQuantity: 0, isActive: true };

export default function ProductForm() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [categories, setCategories] = useState([]);
  const [brands, setBrands] = useState([]);
  const [error, setError] = useState('');
  const [form, setForm] = useState({
    categoryId: '', brandId: '', name: '', description: '', productType: 'Shoes',
    sportType: '', basePrice: 0, isActive: true, imageUrls: [''], variants: [{ ...emptyVariant }],
  });

  useEffect(() => {
    Promise.all([productService.getCategories(), productService.getBrands()]).then(([c, b]) => {
      setCategories(c); setBrands(b);
    });
    if (id) {
      api.get(`/admin/products/${id}`).then((response) => {
        const p = response.data.data;
        setForm({
          categoryId: p.categoryId,
          brandId: p.brandId || '',
          name: p.name,
          description: p.description || '',
          productType: p.productType,
          sportType: p.sportType || '',
          basePrice: p.basePrice,
          isActive: p.isActive,
          imageUrls: p.images?.length ? p.images : [''],
          variants: p.variants?.length ? p.variants.map((v) => ({ ...v, sku: v.sku })) : [{ ...emptyVariant }],
        });
      });
    }
  }, [id]);

  const update = (event) => setForm({ ...form, [event.target.name]: event.target.value });
  const updateVariant = (index, field, value) => {
    const variants = [...form.variants];
    variants[index] = { ...variants[index], [field]: value };
    setForm({ ...form, variants });
  };
  const updateImage = (index, value) => {
    const imageUrls = [...form.imageUrls]; imageUrls[index] = value; setForm({ ...form, imageUrls });
  };

  const submit = async (event) => {
    event.preventDefault();
    setError('');
    const payload = {
      ...form,
      categoryId: Number(form.categoryId),
      brandId: form.brandId ? Number(form.brandId) : null,
      basePrice: Number(form.basePrice),
      imageUrls: form.imageUrls.filter(Boolean),
      variants: form.variants.map((v) => ({ ...v, price: Number(v.price), stockQuantity: Number(v.stockQuantity) })),
    };
    try {
      if (id) await api.put(`/admin/products/${id}`, payload);
      else await api.post('/admin/products', payload);
      navigate('/admin/products');
    } catch (requestError) {
      setError(getErrorMessage(requestError));
    }
  };

  return (
    <div>
      <div className="page-title-row"><h1>{id ? 'Cập nhật sản phẩm' : 'Thêm sản phẩm'}</h1></div>
      <form className="form-card" onSubmit={submit}>
        {error && <div className="alert alert-error">{error}</div>}
        <div className="form-grid-2">
          <div><label>Tên sản phẩm</label><input name="name" value={form.name} onChange={update} required /></div>
          <div><label>Loại sản phẩm</label><select name="productType" value={form.productType} onChange={update}><option value="Shoes">Giày</option><option value="Clothing">Quần áo</option><option value="Equipment">Dụng cụ</option><option value="Accessory">Phụ kiện</option></select></div>
          <div><label>Danh mục</label><select name="categoryId" value={form.categoryId} onChange={update} required><option value="">Chọn danh mục</option>{categories.map((x) => <option key={x.id} value={x.id}>{x.name}</option>)}</select></div>
          <div><label>Thương hiệu</label><select name="brandId" value={form.brandId} onChange={update}><option value="">Không chọn</option>{brands.map((x) => <option key={x.id} value={x.id}>{x.name}</option>)}</select></div>
          <div><label>Giá cơ bản</label><input name="basePrice" type="number" value={form.basePrice} onChange={update} /></div>
          <div><label>Môn thể thao</label><input name="sportType" value={form.sportType} onChange={update} /></div>
        </div>
        <label>Mô tả</label><textarea name="description" value={form.description} onChange={update} />
        <h3>Hình ảnh</h3>
        {form.imageUrls.map((url, index) => <input key={index} value={url} onChange={(e) => updateImage(index, e.target.value)} placeholder="https://..." />)}
        <button type="button" className="btn btn-ghost" onClick={() => setForm({ ...form, imageUrls: [...form.imageUrls, ''] })}>Thêm ảnh</button>
        <h3>Biến thể</h3>
        {form.variants.map((variant, index) => (
          <div className="variant-form-row" key={variant.id || index}>
            <input placeholder="SKU" value={variant.sku} onChange={(e) => updateVariant(index, 'sku', e.target.value)} required />
            <input placeholder="Size" value={variant.size || ''} onChange={(e) => updateVariant(index, 'size', e.target.value)} />
            <input placeholder="Màu" value={variant.color || ''} onChange={(e) => updateVariant(index, 'color', e.target.value)} />
            <input type="number" placeholder="Giá" value={variant.price} onChange={(e) => updateVariant(index, 'price', e.target.value)} required />
            <input type="number" placeholder="Tồn" value={variant.stockQuantity} onChange={(e) => updateVariant(index, 'stockQuantity', e.target.value)} required />
          </div>
        ))}
        <button type="button" className="btn btn-ghost" onClick={() => setForm({ ...form, variants: [...form.variants, { ...emptyVariant }] })}>Thêm biến thể</button>
        <div className="form-actions"><button className="btn btn-primary">Lưu sản phẩm</button></div>
      </form>
    </div>
  );
}
