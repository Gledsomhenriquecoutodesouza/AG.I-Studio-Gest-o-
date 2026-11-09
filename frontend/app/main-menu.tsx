'use client'
import {useEffect,useState} from 'react'

type GroupId='clients'|'finance'
type MenuItem={label:string;tab:string}
const groups:{id:GroupId;label:string;items:MenuItem[]}[]=[
 {id:'clients',label:'Clientes',items:[{label:'Cadastro',tab:'Cadastro'},{label:'Histórico',tab:'Fichas dos clientes'},{label:'Agendamentos',tab:'Agenda'},{label:'Novo agendamento',tab:'Novo agendamento'}]},
 {id:'finance',label:'Financeiro',items:[{label:'Recebimentos',tab:'Financeiro'},{label:'Relatórios',tab:'Relatorios'},{label:'Pacotes mensais',tab:'Pacotes mensais'}]},
]

export default function MainMenu({activeTab,canViewProducts,canViewUsers,productsBlocked,onSelect}:{activeTab:string;canViewProducts:boolean;canViewUsers:boolean;productsBlocked:boolean;onSelect:(tab:string)=>void}){
 const activeGroup=groups.find(group=>group.items.some(item=>item.tab===activeTab))?.id??null
 const [openGroup,setOpenGroup]=useState<GroupId|null>(activeGroup)
 useEffect(()=>{if(activeGroup)setOpenGroup(activeGroup)},[activeGroup])
 function select(tab:string,group?:GroupId){if(group)setOpenGroup(group);onSelect(tab)}
 function groupButton(group:typeof groups[number]){
  const expanded=openGroup===group.id
  const groupActive=group.items.some(item=>item.tab===activeTab)
  return <div key={group.id} className="menu-group"><button type="button" className={`menu-group-toggle ${groupActive?'is-active':''}`} aria-expanded={expanded} onClick={()=>setOpenGroup(expanded?null:group.id)}><span>{group.label}</span><span className={`menu-chevron ${expanded?'is-open':''}`} aria-hidden="true">⌄</span></button>{expanded&&<div className="menu-subitems">{group.items.map(item=><button type="button" key={item.tab} className={`menu-link ${activeTab===item.tab?'is-active':''}`} aria-current={activeTab===item.tab?'page':undefined} onClick={()=>select(item.tab,group.id)}>{item.label}</button>)}</div>}</div>
 }
 return <nav className="card main-menu" aria-label="Menu principal"><button type="button" className={`menu-link menu-overview ${activeTab==='Visao geral'?'is-active':''}`} aria-current={activeTab==='Visao geral'?'page':undefined} onClick={()=>select('Visao geral')}>Visão geral</button>{groups.map(groupButton)}{canViewProducts&&<button type="button" className={`menu-link menu-products ${activeTab==='Produtos'?'is-active':''}`} disabled={productsBlocked} aria-current={activeTab==='Produtos'?'page':undefined} onClick={()=>select('Produtos')}>Produtos</button>}{canViewUsers&&<button type="button" className={`menu-link menu-users ${activeTab==='Usuários'?'is-active':''}`} aria-current={activeTab==='Usuários'?'page':undefined} onClick={()=>select('Usuários')}>Usuários</button>}</nav>
}
