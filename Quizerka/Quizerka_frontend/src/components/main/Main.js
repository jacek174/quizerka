import React, {useEffect, useState} from 'react';
import Question from '../question/Question';
import Results from '../results/Results';

function Main(props) {
    const [state, setstate] = useState([]);
    const [hide, sethide] = useState(true);
    const [points, setpoints] = useState(0);
    const [beforeScore, setbeforeScore] = useState(null);

    useEffect(() => {
        const Questions = () => {
                const requestOptions = {
                    method: 'GET',
                    headers: { 'Content-Type': 'application/json' },
                };
                fetch('http://localhost:5000/get-quiz', requestOptions)
                    .then(response => response.json())
                    .then(data => {console.log(data)
                        setstate(data)
                    });

        }
        Questions();

        const Score = () => {
            let id = localStorage.getItem("id");
            console.log("iddd", id);
            const requestOptions = {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    user_id: id
                })
            };
            fetch('http://localhost:5000/get-score', requestOptions)
                .then(response => response.json())
                .then(data => {console.log("cc", data)
                    setbeforeScore(data.score);
                });

        }
        Score();
    }, [])
    
    const submit_= (e) => {
        e.preventDefault();
        let correct = 0;
        if(e.target.q0.value == state[0].odpowiedz) correct++;
        if(e.target.q1.value == state[1].odpowiedz) correct++;
        if(e.target.q2.value == state[2].odpowiedz) correct++;
        if(e.target.q3.value == state[3].odpowiedz) correct++;
        if(e.target.q4.value == state[4].odpowiedz) correct++;
        setpoints(correct);
        sethide(false);
        console.log(localStorage.getItem("id"))
        console.log("user", localStorage.getItem("id"))
        console.log("scire", correct)

        const requestOptions = {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ 
                "user_id": localStorage.getItem("id"),
                "score": correct
             })
        };
        fetch('http://localhost:5000/set-score', requestOptions)
            .then(response => response.json())
            .then(data => {console.log("XXX", data)
            });
    }
    
    
    return (
        <div className="main-wrapper">
            <div className="main-form">
                <div className={beforeScore == null ? "hide" : "results"}>W poprzednim quizie uzyskałeś {beforeScore}/5</div>
                <form className={hide == false ? "hide" : ""} onSubmit={(e) => submit_(e)}>
                    {state.length == 0 ? <div className="results">Brak danych</div>: state.map((i, index) => {
                        return <Question key={index} name={"q"+index} content={i.pytanie} />
                    })}
                    <input type="submit" value="Wyślij quiz"/>
                    <div className="clear"></div>
                </form>
                <Results hide={hide} points={points} />
            </div>
        </div>
    );
}

export default Main;