import React, { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import { getDocuments } from "../services/documentService"; // Use named import

// Helper to format date nicely
const formatDate = (isoString) => {
  if (!isoString) return "N/A";
  try {
    return new Date(isoString).toLocaleString(); // Use localeString for combined date/time
  } catch (e) {
    console.error("Error formatting date:", isoString, e);
    return "Invalid Date";
  }
};

// Helper to map status number to string (corresponds to ApprovalStatus enum in backend)
const getStatusString = (statusNumber) => {
  const statuses = {
    0: "Pending", // ApprovalStatus.Pending
    1: "Legal Review", // ApprovalStatus.LegalReview
    2: "Manager Review", // ApprovalStatus.ManagerReview
    3: "Approved", // ApprovalStatus.Approved
    4: "Rejected", // ApprovalStatus.Rejected
  };
  return statuses[statusNumber] ?? "Unknown"; // Use ?? for default
};

function Dashboard() {
  const [documents, setDocuments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    const fetchDocuments = async () => {
      setLoading(true);
      setError("");
      try {
        const docs = await getDocuments();
        setDocuments(docs || []); // Ensure documents is always an array
      } catch (err) {
        console.error("Dashboard error fetching documents:", err);
        const message =
          err?.response?.data?.message ||
          err?.message ||
          "Could not fetch documents.";
        setError(message);
      } finally {
        setLoading(false);
      }
    };

    fetchDocuments();
  }, []); // Empty dependency array ensures this runs only once on mount

  if (loading)
    return <div className="p-4 text-center">Loading documents...</div>;
  if (error)
    return <div className="p-4 text-center text-red-600">Error: {error}</div>;

  return (
    <div className="container mx-auto p-4">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-semibold text-gray-700">
          Documents Dashboard
        </h1>
        <Link
          to="/upload"
          className="bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded transition duration-150 ease-in-out"
        >
          Upload New Document
        </Link>
      </div>
      {documents.length === 0 ? (
        <div className="text-center text-gray-500 py-10">
          <p className="text-xl">No documents found.</p>
          <p>Click "Upload New Document" to get started.</p>
        </div>
      ) : (
        <div className="overflow-x-auto bg-white shadow-md rounded-lg">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th
                  scope="col"
                  className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                >
                  File Name
                </th>
                <th
                  scope="col"
                  className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                >
                  Description
                </th>
                <th
                  scope="col"
                  className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                >
                  Uploaded By
                </th>
                <th
                  scope="col"
                  className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                >
                  Uploaded At
                </th>
                <th
                  scope="col"
                  className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                >
                  Status
                </th>
                <th
                  scope="col"
                  className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                >
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {documents.map((doc) => (
                <tr key={doc.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                    {doc.fileName}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {doc.description || "-"}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {doc.uploadedByEmail || "N/A"}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {formatDate(doc.uploadedAt)}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    <span
                      className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${
                        doc.currentStatus === 3
                          ? "bg-green-100 text-green-800"
                          : doc.currentStatus === 4
                          ? "bg-red-100 text-red-800"
                          : "bg-yellow-100 text-yellow-800" // Default for pending/review
                      }`}
                    >
                      {getStatusString(doc.currentStatus)}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                    <Link
                      to={`/documents/${doc.id}`}
                      className="text-indigo-600 hover:text-indigo-900"
                    >
                      View Details
                    </Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

export default Dashboard;
