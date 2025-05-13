import React, { useContext } from "react";
import { Link, useNavigate } from "react-router-dom";
import { AuthContext } from "../context/AuthContext";

function Navbar() {
  const auth = useContext(AuthContext);
  const navigate = useNavigate();

  // Wait for auth context to be initialized
  if (!auth) {
    return (
      <nav className="bg-gray-800 text-white p-4 flex justify-between items-center shadow-md">
        <div>Loading...</div>
      </nav>
    );
  }

  const { user, logout } = auth;

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  return (
    <nav className="bg-gray-800 text-white p-4 flex justify-between items-center shadow-md">
      {/* Left side: Logo/Brand and Links */}
      <div className="flex items-center space-x-4">
        <Link to="/" className="font-bold text-lg hover:text-blue-300">
          DocFlow
        </Link>
        <div className="hidden md:flex items-center space-x-2">
          <Link to="/" className="px-3 py-2 rounded hover:bg-gray-700">
            Dashboard
          </Link>
          {user && (
            <Link to="/upload" className="px-3 py-2 rounded hover:bg-gray-700">
              Upload Document
            </Link>
          )}
          {/* Add more links here as needed */}
        </div>
      </div>

      {/* Right side: Auth status and actions */}
      <div className="flex items-center space-x-4">
        {user ? (
          <>
            <span className="hidden sm:inline">Welcome, {user.email}!</span>
            <button
              onClick={handleLogout}
              className="bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded transition duration-150 ease-in-out"
            >
              Logout
            </button>
          </>
        ) : (
          <>
            <Link to="/login" className="px-3 py-2 rounded hover:bg-gray-700">
              Login
            </Link>
            <Link
              to="/register"
              className="bg-green-500 hover:bg-green-700 text-white font-bold py-2 px-4 rounded transition duration-150 ease-in-out"
            >
              Register
            </Link>
          </>
        )}
      </div>
      {/* Consider adding a mobile menu button here later */}
    </nav>
  );
}

// Hover effect for links can be done with CSS Modules or styled-components for more advanced styling
// For inline styles, you could use onMouseEnter/onMouseLeave, but CSS is preferred.

export default Navbar;
