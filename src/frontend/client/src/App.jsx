import React, { useContext } from "react";
import { Routes, Route, Navigate, Outlet } from "react-router-dom";
import Navbar from "./components/Navbar";
import Login from "./components/Login";
import Register from "./components/Register";
import Dashboard from "./components/Dashboard";
import UploadDocument from "./components/UploadDocument";
import DocumentDetail from "./components/DocumentDetail";
import { AuthContext } from "./context/AuthContext";
import "./App.css"; // Keep default App.css for now

// ProtectedRoute: Renders child routes if authenticated, otherwise redirects to login.
const ProtectedRoute = () => {
  const auth = useContext(AuthContext);

  // It's crucial to handle the initial loading state from AuthProvider
  if (!auth) {
    // Should ideally not happen if used correctly within AuthProvider,
    // but good practice to handle.
    console.error("ProtectedRoute: AuthContext not available!");
    return <Navigate to="/login" replace />;
  }

  if (auth.loading) {
    // Show a loading indicator while AuthProvider checks for existing session
    return <div style={{ padding: "1rem" }}>Checking authentication...</div>;
  }

  // If loading is finished and user is not authenticated, redirect
  if (!auth.isAuthenticated) {
    console.log(
      "ProtectedRoute: User not authenticated, redirecting to login."
    );
    return <Navigate to="/login" replace />;
  }

  // If loading is finished and user is authenticated, render the child route
  return <Outlet />;
};

// PublicRoute: Renders child routes if not authenticated, otherwise redirects to dashboard.
const PublicRoute = () => {
  const auth = useContext(AuthContext);

  if (!auth) {
    console.error("PublicRoute: AuthContext not available!");
    // If auth isn't ready, maybe default to showing the public route (e.g., login)?
    // Or show loading? Let's assume loading covers this.
  }

  if (auth && auth.loading) {
    return <div style={{ padding: "1rem" }}>Loading...</div>;
  }

  // If loading is finished and user IS authenticated, redirect away from public routes (like login/register)
  if (auth && auth.isAuthenticated) {
    console.log(
      "PublicRoute: User is authenticated, redirecting to dashboard."
    );
    return <Navigate to="/" replace />;
  }

  // If loading is finished and user is NOT authenticated, render the child public route (login/register)
  return <Outlet />;
};

function App() {
  return (
    <>
      <Navbar />
      {/* Using a main container for content padding */}
      <main style={{ padding: "1rem 2rem" }}>
        <Routes>
          {/* Routes only accessible when NOT logged in */}
          <Route element={<PublicRoute />}>
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />
          </Route>

          {/* Routes only accessible when logged in */}
          <Route element={<ProtectedRoute />}>
            <Route path="/" element={<Dashboard />} />
            <Route path="/upload" element={<UploadDocument />} />
            <Route path="/documents/:id" element={<DocumentDetail />} />
            {/* Add other protected routes here: */}
            {/* <Route path="/documents/:id" element={<DocumentDetails />} /> */}
            {/* <Route path="/profile" element={<Profile />} /> */}
          </Route>

          {/* Fallback for any unknown paths - redirects to dashboard if logged in, or login if not */}
          {/* This depends on ProtectedRoute/PublicRoute behaviour */}
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </main>
    </>
  );
}

export default App;
