import React from "react";
import "./DarkButton.css";

interface DarkButtonProps {
  text: string;
  onClick?: () => void;
  type?: "button" | "submit" | "reset";
  disabled?: boolean;
}

const DarkButton: React.FC<DarkButtonProps> = ({ text, onClick, type = "button", disabled = false }) => {
  return (
    <button
      type={type}
      onClick={onClick}
      disabled={disabled}
      className="dark-button"
    >
      {text}
    </button>
  );
};

export default DarkButton;