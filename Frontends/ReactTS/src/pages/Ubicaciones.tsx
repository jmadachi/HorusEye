import { useEffect, useState } from 'react';
import api from '../services/api';
import type { Cliente, NodoUbicacion, ApiResponse } from '../types';
import { Plus, Trash2, ChevronRight, ChevronDown } from 'lucide-react';

export default function Ubicaciones() {
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [clienteSeleccionado, setClienteSeleccionado] = useState('');
  const [nodos, setNodos] = useState<NodoUbicacion[]>([]);
  const [showModal, setShowModal] = useState(false);
  const [form, setForm] = useState({ nombre: '', tipoNodo: '', padreId: '', metadata: '' });
  const [expanded, setExpanded] = useState<Set<string>>(new Set());

  const loadClientes = async () => {
    const { data } = await api.get('/api/clientes?pageSize=200') as { data: ApiResponse<{ items: Cliente[] }> };
    if (data.success) setClientes(data.data.items);
  };

  const loadNodos = async () => {
    if (!clienteSeleccionado) { setNodos([]); return; }
    const { data } = await api.get(`/api/ubicaciones?clienteId=${clienteSeleccionado}&pageSize=200`) as { data: ApiResponse<{ items: NodoUbicacion[] }> };
    if (data.success) setNodos(data.data.items);
  };

  useEffect(() => { loadClientes(); }, []);
  useEffect(() => { loadNodos(); }, [clienteSeleccionado]);

  const buildTree = (items: NodoUbicacion[]): NodoUbicacion[] => {
    const map = new Map<string, NodoUbicacion & { children: NodoUbicacion[] }>();
    const roots: (NodoUbicacion & { children: NodoUbicacion[] })[] = [];

    items.forEach(item => map.set(item.id, { ...item, children: [] }));
    items.forEach(item => {
      const node = map.get(item.id)!;
      if (item.padreId && map.has(item.padreId)) {
        map.get(item.padreId)!.children.push(node);
      } else {
        roots.push(node);
      }
    });

    return roots;
  };

  const toggleExpand = (id: string) => {
    setExpanded(prev => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id); else next.add(id);
      return next;
    });
  };

  const save = async () => {
    if (!clienteSeleccionado) return;
    await api.post('/api/ubicaciones', { ...form, clienteId: clienteSeleccionado, padreId: form.padreId || null });
    setShowModal(false); loadNodos();
  };

  const del = async (id: string) => {
    await api.delete(`/api/ubicaciones/${id}`);
    loadNodos();
  };

  const renderNode = (node: NodoUbicacion & { children?: NodoUbicacion[] }, depth = 0) => {
    const hasChildren = node.children && node.children.length > 0;
    const isExpanded = expanded.has(node.id);

    return (
      <div key={node.id}>
        <div className="flex items-center gap-2 py-2 px-2 hover:bg-gray-50 dark:hover:bg-gray-700 rounded" style={{ paddingLeft: `${depth * 24 + 8}px` }}>
          {hasChildren ? (
            <button onClick={() => toggleExpand(node.id)} className="text-gray-500">
              {isExpanded ? <ChevronDown size={16} /> : <ChevronRight size={16} />}
            </button>
          ) : <div className="w-4" />}
          <span className="text-xs px-2 py-0.5 rounded bg-gray-200 dark:bg-gray-600 dark:text-gray-300">{node.tipoNodo}</span>
          <span className="dark:text-gray-200">{node.nombre}</span>
          <span className="text-xs text-gray-400 ml-auto">Nivel {node.nivel}</span>
          <button onClick={() => { setForm({ nombre: '', tipoNodo: '', padreId: node.id, metadata: '' }); setShowModal(true); }} className="text-blue-500 hover:text-blue-700"><Plus size={14} /></button>
          <button onClick={() => del(node.id)} className="text-red-500 hover:text-red-700"><Trash2 size={14} /></button>
        </div>
        {hasChildren && isExpanded && node.children!.map(child => renderNode(child, depth + 1))}
      </div>
    );
  };

  const tree = buildTree(nodos);

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold dark:text-white">Ubicaciones</h2>
        {clienteSeleccionado && (
          <button onClick={() => { setForm({ nombre: '', tipoNodo: '', padreId: '', metadata: '' }); setShowModal(true); }} className="flex items-center gap-2 bg-[#1e3a5f] text-white px-4 py-2 rounded-lg hover:bg-[#2d5a8e] transition">
            <Plus size={16} /> Nueva Ubicacion
          </button>
        )}
      </div>

      <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-4">
        <label className="block text-sm font-medium dark:text-gray-200 mb-1">Cliente</label>
        <select value={clienteSeleccionado} onChange={e => setClienteSeleccionado(e.target.value)} className="w-full max-w-md border dark:border-gray-600 rounded px-3 py-2 dark:bg-gray-700 dark:text-gray-200">
          <option value="">Seleccionar cliente...</option>
          {clientes.map(c => <option key={c.id} value={c.id}>{c.nombre}</option>)}
        </select>
      </div>

      {clienteSeleccionado && (
        <div className="bg-white dark:bg-gray-800 rounded-lg shadow">
          <div className="p-4">
            <h3 className="font-medium dark:text-gray-200 mb-2">Arbol de Ubicaciones</h3>
            {tree.length > 0 ? tree.map(node => renderNode(node)) : (
              <p className="text-gray-400 dark:text-gray-500 text-sm">No hay ubicaciones para este cliente</p>
            )}
          </div>
        </div>
      )}

      {showModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white dark:bg-gray-800 rounded-lg p-6 w-full max-w-md">
            <h3 className="text-lg font-bold mb-4 dark:text-white">Nueva Ubicacion</h3>
            <div className="space-y-3">
              <input placeholder="Nombre (ej: Garaje A)" value={form.nombre} onChange={e => setForm({ ...form, nombre: e.target.value })} className="w-full border dark:border-gray-600 rounded px-3 py-2 dark:bg-gray-700 dark:text-gray-200" />
              <input placeholder="Tipo de nodo (ej: Garaje, Bodega, Sector)" value={form.tipoNodo} onChange={e => setForm({ ...form, tipoNodo: e.target.value })} className="w-full border dark:border-gray-600 rounded px-3 py-2 dark:bg-gray-700 dark:text-gray-200" />
              {form.padreId && <p className="text-sm text-gray-500 dark:text-gray-400">Padre: {nodos.find(n => n.id === form.padreId)?.nombre}</p>}
              <textarea placeholder="Metadata (JSON opcional)" value={form.metadata} onChange={e => setForm({ ...form, metadata: e.target.value })} className="w-full border dark:border-gray-600 rounded px-3 py-2 dark:bg-gray-700 dark:text-gray-200 h-20" />
            </div>
            <div className="flex justify-end gap-2 mt-4">
              <button onClick={() => setShowModal(false)} className="px-4 py-2 text-gray-600 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 rounded">Cancelar</button>
              <button onClick={save} className="px-4 py-2 bg-[#1e3a5f] text-white rounded hover:bg-[#2d5a8e]">Guardar</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
