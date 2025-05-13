import React, { useState, useEffect, useContext } from "react";
import { useParams, Link, useNavigate } from "react-router-dom";
import {
  getDocumentById,
  approveDocument,
  rejectDocument,
} from "../services/documentService";
import { AuthContext } from "../context/AuthContext";
import StatusBadge from "./StatusBadge";

// Helper function (can be moved to a utils file if used elsewhere)
const formatDate = (isoString) => {
  if (!isoString) return "N/A";
  try {
    return new Date(isoString).toLocaleString();
  } catch {
    return "Invalid Date";
  }
};

const getStatusString = (statusNumber) => {
  const statuses = {
    0: "Pending",
    1: "Legal Review",
    2: "Manager Review",
    3: "Approved",
    4: "Rejected",
  };
  return statuses[statusNumber] ?? "Unknown";
};

function DocumentDetail() {
  const { id: documentId } = useParams(); // Get document ID from URL params
  const navigate = useNavigate();
  const auth = useContext(AuthContext);
  const [document, setDocument] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [actionLoading, setActionLoading] = useState(false);
  const [actionError, setActionError] = useState(null);
  const [showRejectComment, setShowRejectComment] = useState(false);
  const [rejectComment, setRejectComment] = useState("");

  useEffect(() => {
    if (!documentId) {
      setError("No document ID provided.");
      setLoading(false);
      return;
    }

    const fetchDocument = async () => {
      setLoading(true);
      setError("");
      try {
        const data = await getDocumentById(documentId);
        setDocument(data);
      } catch (err) {
        console.error(`Error fetching document ${documentId}:`, err);
        setError(err.message || "Failed to fetch document details.");
        if (err.response && err.response.status === 404) {
          setError("Document not found.");
        }
      } finally {
        setLoading(false);
      }
    };

    fetchDocument();
  }, [documentId]); // Re-fetch if documentId changes

  const handleApprove = async () => {
    setActionLoading(true);
    setActionError(null);
    try {
      const updatedDocument = await approveDocument(documentId);
      setDocument(updatedDocument);
      alert("Document approved successfully!");
      setShowRejectComment(false);
      if (
        updatedDocument.currentStatus === "Approved" ||
        updatedDocument.currentStatus === "Rejected"
      ) {
        // No specific navigation for now, but buttons will be disabled by canTakeAction
      }
    } catch (err) {
      const errorMessage =
        err.response?.data?.message ||
        err.message ||
        "Failed to approve document.";
      setActionError(errorMessage);
      alert(`Approval failed: ${errorMessage}`);
    } finally {
      setActionLoading(false);
    }
  };

  const handleReject = async () => {
    if (!showRejectComment) {
      setShowRejectComment(true);
      return;
    }

    if (!rejectComment.trim() && showRejectComment) {
      alert("Please provide a reason for rejection.");
      return;
    }

    setActionLoading(true);
    setActionError(null);
    try {
      const updatedDocument = await rejectDocument(documentId, rejectComment);
      setDocument(updatedDocument);
      alert("Document rejected successfully!");
      setShowRejectComment(false);
      setRejectComment("");
    } catch (err) {
      const errorMessage =
        err.response?.data?.message ||
        err.message ||
        "Failed to reject document.";
      setActionError(errorMessage);
      alert(`Rejection failed: ${errorMessage}`);
    } finally {
      setActionLoading(false);
    }
  };

  if (loading) {
    return <div className="text-center p-8">Loading document details...</div>;
  }

  if (error) {
    return (
      <div className="text-center p-8 text-red-600">
        <p>Error: {error}</p>
        <Link
          to="/"
          className="mt-4 inline-block bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded"
        >
          Go to Dashboard
        </Link>
      </div>
    );
  }

  if (!document) {
    return <div className="text-center p-8">Document not found.</div>;
  }

  // Determine if the current user can take action (simplified)
  // This logic will become more complex based on actual approval steps and roles
  const canTakeAction = () => {
    if (!auth || !auth.user || !document) return false;
    // Example: Only show actions if document is pending or in a review state
    if (
      document.currentStatus === 3 /* Approved */ ||
      document.currentStatus === 4 /* Rejected */
    ) {
      return false;
    }
    // Further role-based checks would go here
    // For now, let's assume an uploader or admin can always see actions as a placeholder
    return (
      auth.user.roles?.includes("Admin") ||
      auth.user.roles?.includes("LegalApprover") ||
      auth.user.roles?.includes("ManagerApprover") ||
      auth.user.roles?.includes("FinalApprover")
    );
  };

  return (
    <div className="container mx-auto p-4 md:p-8">
      <div className="bg-white shadow-xl rounded-lg overflow-hidden">
        <div className="px-6 py-4 bg-gray-100 border-b border-gray-200">
          <h1 className="text-2xl md:text-3xl font-semibold text-gray-800">
            Document: {document.fileName}
          </h1>
        </div>

        <div className="p-6 space-y-6">
          <div>
            <h3 className="text-lg font-medium text-gray-700 mb-2">Details</h3>
            <dl className="grid grid-cols-1 md:grid-cols-2 gap-x-6 gap-y-4 text-sm">
              <div className="col-span-1">
                <dt className="font-semibold text-gray-600">Document ID:</dt>
                <dd className="text-gray-800">{document.id}</dd>
              </div>
              <div className="col-span-1">
                <dt className="font-semibold text-gray-600">File Name:</dt>
                <dd className="text-gray-800">{document.fileName}</dd>
              </div>
              <div className="col-span-1 md:col-span-2">
                <dt className="font-semibold text-gray-600">Description:</dt>
                <dd className="text-gray-800 whitespace-pre-wrap">
                  {document.description || "N/A"}
                </dd>
              </div>
              <div className="col-span-1">
                <dt className="font-semibold text-gray-600">Uploaded By:</dt>
                <dd className="text-gray-800">
                  {document.uploadedByEmail || "N/A"}
                </dd>
              </div>
              <div className="col-span-1">
                <dt className="font-semibold text-gray-600">Uploaded At:</dt>
                <dd className="text-gray-800">
                  {formatDate(document.uploadedAt)}
                </dd>
              </div>
              <div className="col-span-1">
                <dt className="font-semibold text-gray-600">File Size:</dt>
                <dd className="text-gray-800">
                  {(document.fileSize / 1024).toFixed(2)} KB
                </dd>
              </div>
              <div className="col-span-1">
                <dt className="font-semibold text-gray-600">Current Status:</dt>
                <dd>
                  <StatusBadge status={document.currentStatus} />
                </dd>
              </div>
            </dl>
          </div>

          {/* Placeholder for File Preview/Download - this will need more work */}
          <div>
            <h3 className="text-lg font-medium text-gray-700 mb-2">
              File Actions
            </h3>
            {/* Link to download will eventually point to a backend endpoint */}
            <a
              href={
                `#` /* Replace with actual download link: e.g., ${API_BASE_URL}/documents/${document.id}/download */
              }
              target="_blank"
              rel="noopener noreferrer"
              className="inline-block bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded text-sm"
            >
              Download File (Placeholder)
            </a>
            {/* TODO: Add file preview component here if feasible */}
          </div>

          {/* Approval Actions - visibility based on user role and document status */}
          {canTakeAction() && (
            <div className="border-t border-gray-200 pt-6">
              <h3 className="text-lg font-medium text-gray-700 mb-3">
                Approval Actions
              </h3>
              {actionError && (
                <p className="text-red-500 mb-3">Action Error: {actionError}</p>
              )}

              {/* Reject Comment Area - Conditionally Displayed */}
              {showRejectComment && (
                <div className="mb-4">
                  <label
                    htmlFor="rejectComment"
                    className="block text-sm font-medium text-gray-700 mb-1"
                  >
                    Reason for Rejection (Required)
                  </label>
                  <textarea
                    id="rejectComment"
                    rows="3"
                    className="w-full p-2 border border-gray-300 rounded-md shadow-sm focus:ring-indigo-500 focus:border-indigo-500"
                    value={rejectComment}
                    onChange={(e) => setRejectComment(e.target.value)}
                    disabled={actionLoading}
                  ></textarea>
                </div>
              )}

              <div className="flex space-x-3">
                <button
                  onClick={handleApprove}
                  disabled={actionLoading || showRejectComment}
                  className={`bg-green-500 hover:bg-green-700 text-white font-bold py-2 px-4 rounded transition duration-150 ease-in-out ${
                    actionLoading || showRejectComment
                      ? "bg-gray-400 cursor-not-allowed"
                      : "bg-green-600 hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-green-500"
                  }`}
                >
                  {actionLoading ? "Processing..." : "Approve"}
                </button>
                <button
                  onClick={handleReject}
                  disabled={actionLoading}
                  className={`bg-red-500 hover:bg-red-700 text-white font-bold py-2 px-4 rounded transition duration-150 ease-in-out ${
                    actionLoading
                      ? "bg-gray-400 cursor-not-allowed"
                      : showRejectComment
                      ? "bg-red-700 hover:bg-red-800 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500"
                      : "bg-red-500 hover:bg-red-600 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500"
                  }`}
                >
                  {actionLoading
                    ? "Processing..."
                    : showRejectComment
                    ? "Submit Rejection"
                    : "Reject"}
                </button>
                {showRejectComment && (
                  <button
                    onClick={() => {
                      setShowRejectComment(false);
                      setRejectComment("");
                      setActionError(null);
                    }}
                    disabled={actionLoading}
                    className="bg-gray-200 hover:bg-gray-300 text-gray-700 font-semibold py-2 px-4 rounded transition duration-150 ease-in-out"
                  >
                    Cancel
                  </button>
                )}
              </div>
            </div>
          )}

          {/* TODO: Display Approval History/Steps here */}
        </div>
        <div className="px-6 py-3 bg-gray-50 border-t border-gray-200 text-right">
          <Link
            to="/"
            className="text-indigo-600 hover:text-indigo-800 font-medium text-sm"
          >
            &larr; Back to Dashboard
          </Link>
        </div>
      </div>
    </div>
  );
}

export default DocumentDetail;
