import React, { useState } from "react";
import { uploadDocument as apiUploadDocument } from "../services/documentService";
// We don't strictly need useNavigate here if not redirecting after upload
// import { useNavigate } from 'react-router-dom';

function UploadDocument() {
  const [file, setFile] = useState(null);
  const [description, setDescription] = useState("");
  const [error, setError] = useState("");
  const [successMessage, setSuccessMessage] = useState("");
  const [loading, setLoading] = useState(false);
  // const navigate = useNavigate(); // If we want to navigate after upload

  const handleFileChange = (e) => {
    setError("");
    setSuccessMessage("");
    if (e.target.files && e.target.files.length > 0) {
      setFile(e.target.files[0]);
    } else {
      setFile(null);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!file) {
      setError("Please select a file to upload.");
      return;
    }

    setError("");
    setSuccessMessage("");
    setLoading(true);

    try {
      const response = await apiUploadDocument(file, description);
      setSuccessMessage(
        `Document '${
          response.fileName || file.name
        }' uploaded successfully! ID: ${response.id}`
      );
      setFile(null); // Clear selected file
      setDescription(""); // Clear description
      // Reset file input visually
      if (document.getElementById("file-input")) {
        document.getElementById("file-input").value = "";
      }
      // Optionally navigate: navigate('/'); or navigate(`/documents/${response.id}`);
    } catch (err) {
      console.error("Upload component error:", err);
      const message =
        err?.response?.data?.title ||
        err?.response?.data?.message ||
        err?.message ||
        "File upload failed. Please try again.";
      setError(message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="max-w-2xl mx-auto mt-8 p-6 bg-white rounded-lg shadow-md">
      <h2 className="text-2xl font-semibold text-gray-700 text-center mb-6">
        Upload New Document
      </h2>
      <form onSubmit={handleSubmit} className="space-y-6">
        <div>
          <label
            htmlFor="description"
            className="block text-sm font-medium text-gray-700 mb-1"
          >
            Description (Optional):
          </label>
          <input
            type="text"
            id="description"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            placeholder="Enter a brief description for the document"
            className="appearance-none block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm placeholder-gray-400 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
          />
        </div>
        <div>
          <label
            htmlFor="file-input"
            className="block text-sm font-medium text-gray-700 mb-1"
          >
            Document File:
          </label>
          <input
            type="file"
            id="file-input"
            onChange={handleFileChange}
            required
            className="block w-full text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded-md file:border-0 file:text-sm file:font-semibold file:bg-indigo-50 file:text-indigo-700 hover:file:bg-indigo-100"
          />
          {!file && (
            <p className="text-xs text-gray-500 mt-1">Please select a file.</p>
          )}
        </div>

        {error && (
          <div
            className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded relative"
            role="alert"
          >
            <span className="block sm:inline">{error}</span>
          </div>
        )}
        {successMessage && (
          <div
            className="bg-green-100 border border-green-400 text-green-700 px-4 py-3 rounded relative"
            role="alert"
          >
            <span className="block sm:inline">{successMessage}</span>
          </div>
        )}

        <div>
          <button
            type="submit"
            disabled={loading || !file}
            className={`w-full flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 ${
              loading || !file ? "opacity-50 cursor-not-allowed" : ""
            }`}
          >
            {loading ? "Uploading..." : "Upload Document"}
          </button>
        </div>
      </form>
    </div>
  );
}

export default UploadDocument;
