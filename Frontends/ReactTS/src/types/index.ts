export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  userId: string;
  email: string;
  role: string;
  userName: string;
  proveedorId?: string;
  clienteId?: string;
}

export interface User {
  id: string;
  email: string;
  userName: string;
  role: string;
  proveedorId?: string;
  clienteId?: string;
}

export interface Activo {
  id: string;
  placa: string;
  nombre: string;
  categoria: string;
  tenedorResponsable: string | null;
  estadoUbicacion: string;
  tagId: string | null;
  fechaRegistro: string;
}

export interface Tag {
  id: string;
  estado: string;
  fechaRegistro: string;
  fechaActualizacion: string;
}

export interface Movimiento {
  id: number;
  activoId: string;
  activoPlaca: string;
  activoNombre: string;
  puntoLecturaId: string | null;
  tipoMovimiento: string;
  autorizado: boolean;
  alarmaActivada: boolean;
  fechaRegistro: string;
}

export interface KpiData {
  totalActivos: number;
  activosDentro: number;
  activosFuera: number;
  tagsRegistrados: number;
  tagsAsignados: number;
  tagsDisponibles: number;
  ingresosHoy: number;
  salidasHoy: number;
  categorias: { categoria: string; count: number }[];
  fecha: string;
}

export interface Tendencia {
  fecha: string;
  dia: string;
  ingresos: number;
  salidas: number;
  noAutorizadas: number;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors: string[] | null;
  timestamp: string;
}

export interface EventoRfidResponse {
  tagId: string;
  activoPlaca: string | null;
  activoNombre: string | null;
  tipoMovimiento: string;
  autorizado: boolean;
  activarAlarmaSonora: boolean;
  mensaje: string;
  timestamp: string;
}

// === Nuevos tipos RBAC ===

export interface Proveedor {
  id: string;
  nombre: string;
  razonSocial: string;
  ruc: string;
  direccion: string | null;
  telefono: string | null;
  email: string | null;
  activo: boolean;
  fechaRegistro: string;
}

export interface Cliente {
  id: string;
  nombre: string;
  razonSocial: string;
  ruc: string;
  direccion: string | null;
  telefono: string | null;
  email: string | null;
  proveedorId: string | null;
  proveedorNombre: string | null;
  activo: boolean;
  fechaRegistro: string;
}

export interface DispositivoRfid {
  id: string;
  nombre: string;
  fabricante: string;
  modelo: string;
  direccionIP: string | null;
  puerto: number | null;
  ubicacion: string | null;
  clienteId: string | null;
  clienteNombre: string | null;
  proveedorId: string | null;
  proveedorNombre: string | null;
  nodoUbicacionId: string | null;
  nodoUbicacionNombre: string | null;
  tipoDispositivo: string;
  endpointAPI: string | null;
  metodoHTTP: string | null;
  fabricanteDispositivoId: string | null;
  activo: boolean;
  fechaRegistro: string;
}

export interface NodoUbicacion {
  id: string;
  nombre: string;
  tipoNodo: string;
  padreId: string | null;
  clienteId: string;
  nivel: number;
  metadata: string | null;
  activo: boolean;
  children?: NodoUbicacion[];
}

export interface FabricanteDispositivo {
  id: string;
  nombre: string;
  descripcion: string | null;
  urlDocumentacion: string | null;
  endpointEjemplo: string | null;
  activo: boolean;
  fechaRegistro: string;
  campos: CampoPayload[];
}

export interface CampoPayload {
  id: string;
  nombreCampoExterno: string;
  nombreCampoInterno: string;
  tipoDato: string;
  requerido: boolean;
  valorDefecto: string | null;
  ordenExtraccion: number;
}

export const ROLES = {
  ADMIN_SISTEMA: 'Administrador del Sistema',
  ASIST_ADMIN_SISTEMA: 'Asistente del Administrador del Sistema',
  SOPORTE_SISTEMA: 'Soporte del Sistema',
  ADMIN_PROVEEDOR: 'Administrador del Proveedor',
  ASIST_PROVEEDOR: 'Asistente del Proveedor',
  ADMIN_CLIENTE: 'Administrador del Cliente',
  ASIST_CLIENTE: 'Asistente del Cliente',
} as const;

export type RolSistema = typeof ROLES[keyof typeof ROLES];
