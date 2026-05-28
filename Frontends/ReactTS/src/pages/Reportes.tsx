import { useState } from 'react';
import api from '../services/api';
import * as XLSX from 'xlsx';
import { saveAs } from 'file-saver';
import { FileText, Download } from 'lucide-react';

const TIPOS_REPORTE = ['Movimientos', 'Tags Registrados', 'Activos Dentro', 'Activos Fuera'];

export default function Reportes() {
  const [tipoReporte, setTipoReporte] = useState('Movimientos');
  const [fechaInicio, setFechaInicio] = useState(
    new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0]
  );
  const [fechaFin, setFechaFin] = useState(
    new Date().toISOString().split('T')[0]
  );
  const [resultados, setResultados] = useState<unknown[] | null>(null);
  const [loading, setLoading] = useState(false);

  const generarReporte = async () => {
    setLoading(true);
    try {
      const { data } = await api.post('/api/reportes', {
        fechaInicio,
        fechaFin,
        tipoReporte
      });
      if (data.success) setResultados(data.data.datos);
    } catch (err) {
      console.error('Error generating report:', err);
    } finally {
      setLoading(false);
    }
  };

  const exportToExcel = () => {
    if (!resultados || resultados.length === 0) return;

    const ws = XLSX.utils.json_to_sheet(resultados as Record<string, unknown>[]);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'Reporte');

    const excelBuffer = XLSX.write(wb, { bookType: 'xlsx', type: 'array' });
    const blob = new Blob([excelBuffer], { type: 'application/octet-stream' });
    saveAs(blob, `reporte-${tipoReporte}-${fechaInicio}-${fechaFin}.xlsx`);
  };

  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-bold text-gray-800 dark:text-gray-100">Reportes</h2>

      <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-200 mb-1">Tipo de Reporte</label>
            <select
              value={tipoReporte}
              onChange={(e) => setTipoReporte(e.target.value)}
              className="w-full border border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-gray-100 rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-[#1e3a5f]"
            >
              {TIPOS_REPORTE.map((t) => (
                <option key={t} value={t}>{t}</option>
              ))}
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-200 mb-1">Fecha Inicio</label>
            <input
              type="date"
              value={fechaInicio}
              onChange={(e) => setFechaInicio(e.target.value)}
              className="w-full border border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-gray-100 rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-[#1e3a5f]"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-200 mb-1">Fecha Fin</label>
            <input
              type="date"
              value={fechaFin}
              onChange={(e) => setFechaFin(e.target.value)}
              className="w-full border border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-gray-100 rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-[#1e3a5f]"
            />
          </div>
          <div className="flex items-end gap-2">
            <button
              onClick={generarReporte}
              disabled={loading}
              className="flex items-center gap-2 bg-[#1e3a5f] text-white px-4 py-2 rounded hover:bg-[#2d5a8e] transition disabled:opacity-50 cursor-pointer"
            >
              <FileText size={18} /> {loading ? 'Generando...' : 'Generar'}
            </button>
            {resultados && resultados.length > 0 && (
              <button
                onClick={exportToExcel}
                className="flex items-center gap-2 bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700 transition cursor-pointer"
              >
                <Download size={18} /> Excel
              </button>
            )}
          </div>
        </div>

        {resultados && resultados.length > 0 && (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="bg-gray-50 dark:bg-gray-700">
                <tr>
                  {Object.keys(resultados[0] as Record<string, unknown>).map((key) => (
                    <th key={key} className="text-left px-4 py-2 text-gray-600 dark:text-gray-300 text-xs uppercase">
                      {key}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {(resultados as Record<string, unknown>[]).slice(0, 100).map((row, i) => (
                  <tr key={i} className="border-t border-gray-100 dark:border-gray-700">
                    {Object.values(row).map((val, j) => (
                      <td key={j} className="px-4 py-2 text-gray-700 dark:text-gray-200">
                        {String(val ?? '-')}
                      </td>
                    ))}
                  </tr>
                ))}
              </tbody>
            </table>
            {resultados.length > 100 && (
              <p className="text-sm text-gray-400 dark:text-gray-500 text-center py-2">
                Mostrando 100 de {resultados.length} registros. Exporte a Excel para ver todos.
              </p>
            )}
          </div>
        )}

        {resultados && resultados.length === 0 && (
          <p className="text-center text-gray-400 dark:text-gray-500 py-8">No se encontraron resultados</p>
        )}
      </div>
    </div>
  );
}
