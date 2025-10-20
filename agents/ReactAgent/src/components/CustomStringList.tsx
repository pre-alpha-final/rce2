import "./CustomStringList.css";

interface CustomStringListProps {
  items: string[];
  title?: string;
}

export default function CustomStringList({
  items,
  title,
}: CustomStringListProps) {
  return (
    <div className="custom-string-list-container">
      {title && <h2 className="string-list-title">{title}</h2>}
      <ul className="custom-string-list">
        {items.map((item, index) => (
          <li key={index} className="custom-string-list-item">
            {item}
          </li>
        ))}
      </ul>
    </div>
  );
}
