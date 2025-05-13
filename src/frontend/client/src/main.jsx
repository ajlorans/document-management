import React from "react";
import ReactDOM from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import App from "./App.jsx";
import { AuthProvider } from "./context/AuthContext.jsx";
import "./index.css"; // Keep default index.css for now

ReactDOM.createRoot(document.getElementById("root")).render(
  // <React.StrictMode> // StrictMode can cause double rendering in dev, uncomment later if needed
  <BrowserRouter>
    <AuthProvider>
      <App />
    </AuthProvider>
  </BrowserRouter>
  // </React.StrictMode>,
);
