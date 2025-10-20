import "./CustomButton.css";

interface CustomButtonProps {
  text: string;
  onClick?: () => void;
  type?: "button" | "submit" | "reset";
  disabled?: boolean;
}

export default function CustomButton({
  text,
  onClick,
  type = "button",
  disabled = false,
}: CustomButtonProps) {
  return (
    <button
      type={type}
      onClick={onClick}
      disabled={disabled}
      className="custom-button"
    >
      {text}
    </button>
  );
}
