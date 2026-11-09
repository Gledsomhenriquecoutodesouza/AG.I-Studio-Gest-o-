'use client'

type Product={id:number;name:string;price:number;stock:number}
const money=(value:number)=>value.toLocaleString('pt-BR',{style:'currency',currency:'BRL'})

export default function ProductCatalogPanel({products}:{products:Product[]}){
 const available=products
 return <section className="card product-catalog" aria-labelledby="product-catalog-title"><div className="mb-5"><h3 id="product-catalog-title" className="text-lg font-semibold">Produtos disponíveis</h3><p className="mt-1 text-sm text-[#756773]">Consulte os produtos cadastrados no salão.</p></div>{available.length===0?<p className="rounded-xl border border-dashed p-6 text-center text-sm text-[#756773]">Nenhum produto disponível no momento.</p>:<ul className="grid gap-3 sm:grid-cols-2 xl:grid-cols-3">{available.map(product=><li key={product.id} className="product-catalog-item"><span className="product-catalog-name">{product.name}</span><span className="product-catalog-price">{money(product.price)}</span></li>)}</ul>}</section>
}
