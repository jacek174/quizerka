import React from 'react';

function Question({name, content}) {
    return (
        <div className="question">
            <label htmlFor={name}>{content}</label>
            <textarea type="text" name={name} id={name}/>
        </div>
    );
}

export default Question;