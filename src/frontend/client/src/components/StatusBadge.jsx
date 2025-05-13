import React from "react";

const StatusBadge = ({ status }) => {
  let badgeColor = "bg-gray-200 text-gray-800"; // Default
  let dotColor = "bg-gray-500";

  if (status) {
    const lowerStatus = status.toLowerCase();
    if (lowerStatus.includes("approved")) {
      badgeColor = "bg-green-100 text-green-800";
      dotColor = "bg-green-500";
    } else if (lowerStatus.includes("rejected")) {
      badgeColor = "bg-red-100 text-red-800";
      dotColor = "bg-red-500";
    } else if (
      lowerStatus.includes("pending") ||
      lowerStatus.includes("awaiting")
    ) {
      badgeColor = "bg-yellow-100 text-yellow-800";
      dotColor = "bg-yellow-500";
    } else if (lowerStatus.includes("progress")) {
      badgeColor = "bg-blue-100 text-blue-800";
      dotColor = "bg-blue-500";
    }
  }

  return (
    <span
      className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${badgeColor}`}
    >
      <svg
        className={`mr-1.5 h-2 w-2 ${dotColor} rounded-full`}
        viewBox="0 0 8 8"
        fill="currentColor"
        aria-hidden="true"
      >
        <circle cx="4" cy="4" r="3" />
      </svg>
      {status || "Unknown"}
    </span>
  );
};

export default StatusBadge;
