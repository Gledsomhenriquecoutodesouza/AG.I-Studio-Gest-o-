const base=process.env.NEXT_PUBLIC_API_URL??'http://localhost:5000/api'
export async function api<T>(path:string,init?:RequestInit):Promise<T>{const r=await fetch(`${base}${path}`,{...init,headers:{'Content-Type':'application/json',...init?.headers},cache:'no-store'});if(!r.ok){const msg=await r.text();throw new Error(msg||`Erro HTTP ${r.status}`)}return r.status===204?undefined as T:r.json()}
