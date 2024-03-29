import "./NavBar.css";
import LoginButton from "./LoginButton";
import LogoutButton from "./LogoutButton";
import { useAuth0 } from "@auth0/auth0-react";
import { useNavigate } from "react-router-dom";
import { AuthService } from "./AuthService";

export default function NavBar() {
  const { isAuthenticated, user } = useAuth0();
  const navigate = useNavigate();

  return (
    <div className="navBar">
      <div className="leftSide">
        <p className="name">MKS Courier</p>
      </div>
      <div className="rightSide">
        <button onClick={() => navigate("/")}>Home</button>
        {isAuthenticated && (
          <button onClick={() => navigate("/requests")}>Requests</button>
        )}

        {!isAuthenticated && <LoginButton />}
        {isAuthenticated && (
          <button onClick={() => navigate("/profile")}>Profile</button>
        )}
        {isAuthenticated && <LogoutButton />}

        {isAuthenticated && AuthService.isCourier(user) && (
          <button onClick={() => navigate("/deliveries")}>Deliveries</button>
        )}
        {isAuthenticated && AuthService.isCourier(user) && (
          <button onClick={() => navigate("/my-deliveries")}>
            My deliveries
          </button>
        )}

        {isAuthenticated && AuthService.isOfficeWorker(user) && (
          <button onClick={() => navigate("/inquiries")}>Inquiries</button>
        )}
        {isAuthenticated && AuthService.isOfficeWorker(user) && (
          <button onClick={() => navigate("/offers")}>Offers</button>
        )}
      </div>
    </div>
  );
}
