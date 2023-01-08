import './App.css';
import {useState, useEffect} from 'react';
import {
  BrowserRouter as Router,
  Routes as Switch,
  Route
} from "react-router-dom";
import Main from './components/main/Main';
import NotFound from './components/notfound/NotFound';
import Login from './components/login/Login';
import Register from './components/register/Register';
import Nav from './components/nav/Nav';


function App() {
  const [isLogged, setIsLogged] = useState(false);
  const [id, setid] = useState(localStorage.getItem('id'));
  let render;

  useEffect(() => {
    function checkLoginStatus() {
      if(id != null) {
        setIsLogged(false);
      }
    }
    checkLoginStatus();
  }, []);

  let loggedContent = () => {return (<div className="wrapper">
                                        <Nav setIsLogged={setIsLogged} />
                                        <div className="content">
                                          <Switch>
                                              <Route exact path='/' element={<Main />} />
                                              <Route path='*' element={<NotFound/>} />
                                          </Switch>
                                        </div>
                                      </div>)};

  let notLoggedContent = () => {return (<div className="wrapper">
                                          <Switch>
                                              <Route exact path='/' element={<Login setIsLogged={setIsLogged} />}/>
                                              <Route exact path='/register' element={<Register />}/>
                                              <Route path='*' element={<NotFound/>} />
                                          </Switch>
                                        </div>)};
  if(!isLogged) {
    render = notLoggedContent();
  } else {
    render = loggedContent();
  }



  return (
    <Router>
      { render }
    </Router>
  );
}

export default App;
