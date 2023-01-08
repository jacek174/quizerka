import React from 'react';

function Results({hide, points}) {
    return (
        <div className={hide == true ? "hide" : ""}>
            <div className="results">Otrzymałeś {points} punktów na 5 możliwych</div>
        </div>
        
    );
}

export default Results;