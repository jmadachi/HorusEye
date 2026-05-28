import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import { ThemeProvider } from './context/ThemeContext';
import ProtectedRoute from './components/ProtectedRoute';
import Layout from './components/Layout';
import Login from './pages/Login';
import Dashboard from './pages/Dashboard';
import Activos from './pages/Activos';
import TagsPage from './pages/Tags';
import Reportes from './pages/Reportes';
import Usuarios from './pages/Usuarios';

function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <ThemeProvider>
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route
            element={
              <ProtectedRoute>
                <Layout />
              </ProtectedRoute>
            }
          >
            <Route path="/dashboard" element={<Dashboard />} />
            <Route path="/activos" element={<Activos />} />
            <Route path="/tags" element={<TagsPage />} />
            <Route path="/reportes" element={<Reportes />} />
            <Route path="/usuarios" element={<Usuarios />} />
          </Route>
          <Route path="*" element={<Navigate to="/dashboard" replace />} />
        </Routes>
        </ThemeProvider>
      </AuthProvider>
    </BrowserRouter>
  );
}

export default App;
